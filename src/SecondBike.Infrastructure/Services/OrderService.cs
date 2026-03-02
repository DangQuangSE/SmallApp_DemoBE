using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Transaction Hub — Order placement, deposit, and payment processing.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<BikePost> _postRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly IUnitOfWork _uow;

    public OrderService(
        IRepository<Order> orderRepo,
        IRepository<BikePost> postRepo,
        IRepository<Payment> paymentRepo,
        IRepository<AppUser> userRepo,
        IUnitOfWork uow)
    {
        _orderRepo = orderRepo;
        _postRepo = postRepo;
        _paymentRepo = paymentRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<OrderDto>> PlaceOrderAsync(Guid buyerId, CreateOrderDto dto, CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(dto.BikePostId, ct);
        if (post is null) return Result<OrderDto>.Failure("Bike post not found");
        if (post.Status != PostStatus.Active) return Result<OrderDto>.Failure("This bike is not available for purchase");
        if (post.SellerId == buyerId) return Result<OrderDto>.Failure("You cannot buy your own bike");

        var depositPct = Math.Clamp(dto.DepositPercentage, 10, 30);
        var depositAmount = Math.Round(post.Price * depositPct / 100, 2);

        var order = new Order
        {
            OrderNumber = $"SB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            BuyerId = buyerId,
            SellerId = post.SellerId,
            BikePostId = post.Id,
            BikePrice = post.Price,
            DepositPercentage = depositPct,
            DepositAmount = depositAmount,
            RemainingAmount = post.Price - depositAmount,
            TotalAmount = post.Price,
            ShippingAddress = dto.ShippingAddress,
            Status = OrderStatus.Pending
        };

        await _orderRepo.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        var buyer = await _userRepo.GetByIdAsync(buyerId, ct);
        var seller = await _userRepo.GetByIdAsync(post.SellerId, ct);

        return Result<OrderDto>.Success(MapToDto(order, post, buyer!, seller!));
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result<OrderDto>.Failure("Order not found");

        var post = await _postRepo.GetByIdAsync(order.BikePostId, ct);
        var buyer = await _userRepo.GetByIdAsync(order.BuyerId, ct);
        var seller = await _userRepo.GetByIdAsync(order.SellerId, ct);

        return Result<OrderDto>.Success(MapToDto(order, post!, buyer!, seller!));
    }

    public async Task<Result<List<OrderDto>>> GetByBuyerAsync(Guid buyerId, CancellationToken ct = default)
    {
        var orders = await _orderRepo.FindAsync(o => o.BuyerId == buyerId, ct);
        return await MapOrderListAsync(orders, ct);
    }

    public async Task<Result<List<OrderDto>>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default)
    {
        var orders = await _orderRepo.FindAsync(o => o.SellerId == sellerId, ct);
        return await MapOrderListAsync(orders, ct);
    }

    public async Task<Result> CancelOrderAsync(Guid userId, Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != userId && order.SellerId != userId)
            return Result.Failure("Access denied");
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Refunded)
            return Result.Failure("Cannot cancel this order");

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ConfirmDeliveryAsync(Guid buyerId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result.Failure("Access denied");

        order.Status = OrderStatus.Completed;
        order.DeliveredAt = DateTime.UtcNow;
        order.CompletedAt = DateTime.UtcNow;

        // Mark post as sold
        var post = await _postRepo.GetByIdAsync(order.BikePostId, ct);
        if (post is not null)
        {
            post.Status = PostStatus.Sold;
            _postRepo.Update(post);
        }

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ProcessPaymentAsync(Guid buyerId, ProcessPaymentDto dto, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result.Failure("Access denied");

        var amount = dto.Type == PaymentType.Deposit ? order.DepositAmount : order.RemainingAmount;

        var payment = new Payment
        {
            OrderId = order.Id,
            TransactionId = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Amount = amount,
            Type = dto.Type,
            Method = dto.Method,
            Gateway = dto.Gateway,
            Status = PaymentStatus.Completed,
            ProcessedAt = DateTime.UtcNow
        };

        await _paymentRepo.AddAsync(payment, ct);

        if (dto.Type == PaymentType.Deposit)
        {
            order.Status = OrderStatus.DepositPaid;
            order.DepositPaidAt = DateTime.UtcNow;
        }
        else
        {
            order.Status = OrderStatus.FullyPaid;
            order.FullPaymentAt = DateTime.UtcNow;
        }

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> OpenDisputeAsync(Guid buyerId, Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result.Failure("Access denied");

        order.HasDispute = true;
        order.DisputeReason = reason;
        order.Status = OrderStatus.Disputed;

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<List<OrderDto>>> MapOrderListAsync(IReadOnlyList<Order> orders, CancellationToken ct)
    {
        var dtos = new List<OrderDto>();
        foreach (var order in orders)
        {
            var post = await _postRepo.GetByIdAsync(order.BikePostId, ct);
            var buyer = await _userRepo.GetByIdAsync(order.BuyerId, ct);
            var seller = await _userRepo.GetByIdAsync(order.SellerId, ct);
            dtos.Add(MapToDto(order, post!, buyer!, seller!));
        }
        return Result<List<OrderDto>>.Success(dtos);
    }

    private static OrderDto MapToDto(Order order, BikePost post, AppUser buyer, AppUser seller)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            BikePrice = order.BikePrice,
            DepositAmount = order.DepositAmount,
            RemainingAmount = order.RemainingAmount,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            TrackingNumber = order.TrackingNumber,
            CreatedAt = order.CreatedAt,
            BikeTitle = post.Title,
            BikeImageUrl = post.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                        ?? post.Images.FirstOrDefault()?.ImageUrl,
            BuyerName = buyer.FullName,
            SellerName = seller.FullName
        };
    }
}
