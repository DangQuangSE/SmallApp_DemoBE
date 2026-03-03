using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Transaction Hub — Order placement and payment processing.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<ListingMedium> _mediaRepo;
    private readonly IUnitOfWork _uow;

    public OrderService(
        IRepository<Order> orderRepo,
        IRepository<BicycleListing> listingRepo,
        IRepository<Payment> paymentRepo,
        IRepository<User> userRepo,
        IRepository<ListingMedium> mediaRepo,
        IUnitOfWork uow)
    {
        _orderRepo = orderRepo;
        _listingRepo = listingRepo;
        _paymentRepo = paymentRepo;
        _userRepo = userRepo;
        _mediaRepo = mediaRepo;
        _uow = uow;
    }

    public async Task<Result<OrderDto>> PlaceOrderAsync(int buyerId, CreateOrderDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null) return Result<OrderDto>.Failure("Listing not found");
        if (listing.ListingStatus != 1) return Result<OrderDto>.Failure("This listing is not available");
        if (listing.SellerId == buyerId) return Result<OrderDto>.Failure("You cannot buy your own listing");

        var order = new Order
        {
            BuyerId = buyerId,
            ListingId = listing.ListingId,
            TotalAmount = listing.Price,
            OrderStatus = 1, // Pending
            OrderDate = DateTime.UtcNow
        };

        await _orderRepo.AddAsync(order, ct);

        // Mark listing as sold
        listing.ListingStatus = 3; // Sold
        _listingRepo.Update(listing);

        await _uow.SaveChangesAsync(ct);
        return Result<OrderDto>.Success(await BuildOrderDtoAsync(order, ct));
    }

    public async Task<Result<OrderDto>> GetByIdAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result<OrderDto>.Failure("Order not found");
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
        if (order.OrderStatus is 4 or 5) // Completed or Cancelled
            return Result.Failure("Cannot cancel this order");

        order.OrderStatus = 5; // Cancelled
        _orderRepo.Update(order);

        // Restore listing
        var listing = await _listingRepo.GetByIdAsync(order.ListingId, ct);
        if (listing is not null)
        {
            listing.ListingStatus = 1; // Active
            _listingRepo.Update(listing);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ConfirmDeliveryAsync(int buyerId, int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result.Failure("Access denied");

        order.OrderStatus = 4; // Completed
        _orderRepo.Update(order);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ProcessPaymentAsync(int buyerId, ProcessPaymentDto dto, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result.Failure("Order not found");
        if (order.BuyerId != buyerId) return Result.Failure("Access denied");

        var payment = new Payment
        {
            OrderId = order.OrderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            TransactionRef = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            PaymentDate = DateTime.UtcNow
        };

        await _paymentRepo.AddAsync(payment, ct);

        order.OrderStatus = 2; // Paid
        _orderRepo.Update(order);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
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

        return new OrderDto
        {
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            BikeTitle = listing?.Title ?? "Unknown",
            BikeImageUrl = bikeImageUrl,
            BuyerName = buyer?.Username ?? "Unknown",
            SellerName = sellerName
        };
    }
}
