using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Transaction Hub — Order placement, 20% deposit, and VNPay payment processing.
/// 
/// Flow:
///   1. Buyer calls PlaceOrder ? Order created (status=Pending), Deposit created (status=Pending, amount=20%)
///   2. Buyer calls CreatePaymentUrl (type="deposit") ? VNPay redirect URL for 20% deposit
///   3. VNPay callback ? Deposit confirmed, Order status ? Paid (deposit)
///   4. Buyer calls CreatePaymentUrl (type="full") ? VNPay redirect URL for remaining 80%
///   5. VNPay callback ? Full payment recorded, Order status ? Shipping
///   6. Buyer calls ConfirmDelivery ? Order status ? Completed
/// </summary>
public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<Deposit> _depositRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<ListingMedium> _mediaRepo;
    private readonly IUnitOfWork _uow;
    private readonly IVnPayService _vnPayService;
    private readonly ILogger<OrderService> _logger;

    private const decimal DepositRate = 0.20m;

    public OrderService(
        IRepository<Order> orderRepo,
        IRepository<BicycleListing> listingRepo,
        IRepository<Payment> paymentRepo,
        IRepository<Deposit> depositRepo,
        IRepository<User> userRepo,
        IRepository<ListingMedium> mediaRepo,
        IUnitOfWork uow,
        IVnPayService vnPayService,
        ILogger<OrderService> logger)
    {
        _orderRepo = orderRepo;
        _listingRepo = listingRepo;
        _paymentRepo = paymentRepo;
        _depositRepo = depositRepo;
        _userRepo = userRepo;
        _mediaRepo = mediaRepo;
        _uow = uow;
        _vnPayService = vnPayService;
        _logger = logger;
    }

    /// <summary>
    /// UC1: Place Order — create order + deposit record (20% of listing price).
    /// </summary>
    public async Task<Result<OrderDto>> PlaceOrderAsync(int buyerId, CreateOrderDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null) return Result<OrderDto>.Failure("Listing not found");
        if (listing.ListingStatus != 1) return Result<OrderDto>.Failure("This listing is not available");
        if (listing.SellerId == buyerId) return Result<OrderDto>.Failure("You cannot buy your own listing");

        // Check duplicate pending order
        var existingOrders = await _orderRepo.FindAsync(
            o => o.BuyerId == buyerId && o.ListingId == dto.ListingId && o.OrderStatus == 1, ct);
        if (existingOrders.Any())
            return Result<OrderDto>.Failure("You already have a pending order for this listing");

        var totalAmount = listing.Price;
        var depositAmount = Math.Round(totalAmount * DepositRate, 0);

        // Create order
        var order = new Order
        {
            BuyerId = buyerId,
            ListingId = listing.ListingId,
            TotalAmount = totalAmount,
            OrderStatus = 1, // Pending
            OrderDate = DateTime.UtcNow
        };
        await _orderRepo.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        // Create deposit record
        var deposit = new Deposit
        {
            OrderId = order.OrderId,
            Amount = depositAmount,
            Status = 1, // Pending
            DepositDate = DateTime.UtcNow
        };
        await _depositRepo.AddAsync(deposit, ct);

        // Mark listing as sold (reserve)
        listing.ListingStatus = 3; // Sold
        _listingRepo.Update(listing);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Order {OrderId} placed by buyer {BuyerId} for listing {ListingId}. Total: {Total}, Deposit: {Deposit}",
            order.OrderId, buyerId, listing.ListingId, totalAmount, depositAmount);

        return Result<OrderDto>.Success(await BuildOrderDtoAsync(order, ct));
    }

    /// <summary>
    /// UC2: Create VNPay payment URL for deposit (20%) or full remaining (80%).
    /// </summary>
    public async Task<Result<PaymentUrlResultDto>> CreatePaymentUrlAsync(
        int buyerId, CreatePaymentUrlDto dto, string clientIp, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result<PaymentUrlResultDto>.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result<PaymentUrlResultDto>.Failure("Access denied");

        var paymentType = dto.PaymentType.ToLowerInvariant();
        decimal amount;
        string orderInfo;

        if (paymentType == "deposit")
        {
            if (order.OrderStatus != 1) // must be Pending
                return Result<PaymentUrlResultDto>.Failure("Order is not in pending status for deposit payment");

            var deposits = await _depositRepo.FindAsync(d => d.OrderId == order.OrderId, ct);
            var pendingDeposit = deposits.FirstOrDefault(d => d.Status == 1);
            if (pendingDeposit is null)
                return Result<PaymentUrlResultDto>.Failure("No pending deposit found");

            amount = pendingDeposit.Amount ?? 0;
            if (amount <= 0)
                return Result<PaymentUrlResultDto>.Failure("Invalid deposit amount");

            orderInfo = $"SecondBike - Dat coc don hang #{order.OrderId}";
        }
        else if (paymentType == "full")
        {
            if (order.OrderStatus != 2) // must be Paid (deposit paid)
                return Result<PaymentUrlResultDto>.Failure("Deposit must be paid first");

            var totalPaid = await GetTotalPaidAsync(order.OrderId, ct);
            amount = (order.TotalAmount ?? 0) - totalPaid;
            if (amount <= 0)
                return Result<PaymentUrlResultDto>.Failure("Order is already fully paid");

            orderInfo = $"SecondBike - Thanh toan don hang #{order.OrderId}";
        }
        else
        {
            return Result<PaymentUrlResultDto>.Failure("PaymentType must be 'deposit' or 'full'");
        }

        var paymentUrl = _vnPayService.CreatePaymentUrl(order.OrderId, amount, orderInfo, clientIp);

        _logger.LogInformation(
            "VNPay payment URL created for order {OrderId}, type={PaymentType}, amount={Amount}",
            order.OrderId, paymentType, amount);

        return Result<PaymentUrlResultDto>.Success(new PaymentUrlResultDto
        {
            PaymentUrl = paymentUrl,
            OrderId = order.OrderId,
            Amount = amount,
            PaymentType = paymentType
        });
    }

    /// <summary>
    /// Process VNPay IPN/Return callback — validate signature, record payment, update order status.
    /// </summary>
    public async Task<Result<OrderDto>> ProcessVnPayCallbackAsync(
        IDictionary<string, string> queryParams, CancellationToken ct = default)
    {
        // Validate VNPay signature
        if (!_vnPayService.ValidateCallback(queryParams))
        {
            _logger.LogWarning("VNPay callback signature validation failed");
            return Result<OrderDto>.Failure("Invalid signature");
        }

        var responseCode = queryParams.TryGetValue("vnp_ResponseCode", out var rc) ? rc : "";
        var txnRef = queryParams.TryGetValue("vnp_TxnRef", out var tr) ? tr : "";
        var transactionNo = queryParams.TryGetValue("vnp_TransactionNo", out var tn) ? tn : "";
        var vnpAmount = queryParams.TryGetValue("vnp_Amount", out var amtStr) && long.TryParse(amtStr, out var a) ? a : 0L;
        var amount = vnpAmount / 100m; // VNPay sends amount * 100

        // Parse orderId from txnRef (format: "{orderId}-{timestamp}")
        var orderIdStr = txnRef.Split('-').FirstOrDefault();
        if (!int.TryParse(orderIdStr, out var orderId))
            return Result<OrderDto>.Failure("Invalid transaction reference");

        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result<OrderDto>.Failure("Order not found");

        // Check if already processed (idempotency)
        var existingPayments = await _paymentRepo.FindAsync(p => p.TransactionRef == txnRef, ct);
        if (existingPayments.Any())
        {
            _logger.LogInformation("VNPay callback already processed for txnRef={TxnRef}", txnRef);
            return Result<OrderDto>.Success(await BuildOrderDtoAsync(order, ct));
        }

        if (responseCode != "00")
        {
            _logger.LogWarning("VNPay payment failed for order {OrderId}. ResponseCode={Code}", orderId, responseCode);
            return Result<OrderDto>.Failure($"Payment failed (VNPay response code: {responseCode})");
        }

        // Record payment
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = "VNPay",
            TransactionRef = txnRef,
            PaymentDate = DateTime.UtcNow
        };
        await _paymentRepo.AddAsync(payment, ct);

        // Determine if this is deposit or full payment, then update status
        var totalPaidBefore = await GetTotalPaidAsync(orderId, ct);
        var totalPaidAfter = totalPaidBefore + amount;
        var totalAmount = order.TotalAmount ?? 0;

        if (order.OrderStatus == 1) // Pending -> paying deposit
        {
            // Mark deposit as confirmed
            var deposits = await _depositRepo.FindAsync(d => d.OrderId == orderId && d.Status == 1, ct);
            var deposit = deposits.FirstOrDefault();
            if (deposit is not null)
            {
                deposit.Status = 2; // Confirmed
                _depositRepo.Update(deposit);
            }

            order.OrderStatus = 2; // Paid (deposit paid)
            _orderRepo.Update(order);

            _logger.LogInformation("Deposit confirmed for order {OrderId}. Amount={Amount}", orderId, amount);
        }
        else if (order.OrderStatus == 2) // Paid (deposit) -> paying remaining
        {
            if (totalPaidAfter >= totalAmount)
            {
                order.OrderStatus = 3; // Shipping (fully paid)
                _orderRepo.Update(order);

                _logger.LogInformation("Full payment completed for order {OrderId}. Total paid={TotalPaid}", orderId, totalPaidAfter);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result<OrderDto>.Success(await BuildOrderDtoAsync(order, ct));
    }

    public async Task<Result<OrderDto>> GetByIdAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result<OrderDto>.Failure("Order not found");

        // Allow buyer or seller to view the order
        var listing = await _listingRepo.GetByIdAsync(order.ListingId, ct);
        if (order.BuyerId != userId && listing?.SellerId != userId)
            return Result<OrderDto>.Failure("Access denied");

        return Result<OrderDto>.Success(await BuildOrderDtoAsync(order, ct));
    }

    public async Task<Result<List<OrderDto>>> GetByBuyerAsync(int buyerId, CancellationToken ct = default)
    {
        var orders = await _orderRepo.FindAsync(o => o.BuyerId == buyerId, ct);
        var dtos = new List<OrderDto>();
        foreach (var order in orders.OrderByDescending(o => o.OrderDate))
        {
            dtos.Add(await BuildOrderDtoAsync(order, ct));
        }
        return Result<List<OrderDto>>.Success(dtos);
    }

    public async Task<Result> CancelOrderAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != userId) return Result.Failure("Access denied");
        if (order.OrderStatus is 3 or 4 or 5) // Shipping, Completed, or Cancelled
            return Result.Failure("Cannot cancel this order");

        order.OrderStatus = 5; // Cancelled
        _orderRepo.Update(order);

        // Restore listing to active
        var listing = await _listingRepo.GetByIdAsync(order.ListingId, ct);
        if (listing is not null)
        {
            listing.ListingStatus = 1; // Active
            _listingRepo.Update(listing);
        }

        // Mark pending deposits as cancelled
        var deposits = await _depositRepo.FindAsync(d => d.OrderId == orderId && d.Status == 1, ct);
        foreach (var deposit in deposits)
        {
            deposit.Status = 3; // Cancelled
            _depositRepo.Update(deposit);
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);

        return Result.Success();
    }

    public async Task<Result> ConfirmDeliveryAsync(int buyerId, int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result.Failure("Access denied");
        if (order.OrderStatus != 3) // Must be Shipping
            return Result.Failure("Order must be in shipping status to confirm delivery");

        order.OrderStatus = 4; // Completed
        _orderRepo.Update(order);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} delivery confirmed by buyer {BuyerId}", orderId, buyerId);

        return Result.Success();
    }

    #region Private Helpers

    private async Task<decimal> GetTotalPaidAsync(int orderId, CancellationToken ct)
    {
        var payments = await _paymentRepo.FindAsync(p => p.OrderId == orderId, ct);
        return payments.Sum(p => p.Amount ?? 0);
    }

    private async Task<OrderDto> BuildOrderDtoAsync(Order order, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(order.ListingId, ct);
        var buyer = await _userRepo.GetByIdAsync(order.BuyerId, ct);

        string sellerName = "Unknown";
        string? bikeImageUrl = null;

        if (listing is not null)
        {
            var seller = await _userRepo.GetByIdAsync(listing.SellerId, ct);
            sellerName = seller?.Username ?? "Unknown";

            var media = await _mediaRepo.FindAsync(m => m.ListingId == listing.ListingId && m.IsThumbnail == true, ct);
            bikeImageUrl = media.FirstOrDefault()?.MediaUrl;
        }

        // Load deposit info
        var deposits = await _depositRepo.FindAsync(d => d.OrderId == order.OrderId, ct);
        var deposit = deposits.FirstOrDefault();

        // Load payment records
        var payments = await _paymentRepo.FindAsync(p => p.OrderId == order.OrderId, ct);
        var totalPaid = payments.Sum(p => p.Amount ?? 0);

        return new OrderDto
        {
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            DepositAmount = deposit?.Amount,
            DepositStatus = deposit?.Status,
            RemainingAmount = (order.TotalAmount ?? 0) - totalPaid,
            OrderDate = order.OrderDate,
            BikeTitle = listing?.Title ?? "Unknown",
            BikeImageUrl = bikeImageUrl,
            BuyerName = buyer?.Username ?? "Unknown",
            SellerName = sellerName,
            Payments = payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionRef = p.TransactionRef,
                PaymentDate = p.PaymentDate
            }).OrderByDescending(p => p.PaymentDate).ToList()
        };
    }

    #endregion
}
