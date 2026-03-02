using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Orders;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for orders and payments (Transaction Hub — Team Member 2).
/// </summary>
public interface IOrderService
{
    Task<Result<OrderDto>> PlaceOrderAsync(Guid buyerId, CreateOrderDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Result<List<OrderDto>>> GetByBuyerAsync(Guid buyerId, CancellationToken ct = default);
    Task<Result<List<OrderDto>>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default);
    Task<Result> CancelOrderAsync(Guid userId, Guid orderId, string reason, CancellationToken ct = default);
    Task<Result> ConfirmDeliveryAsync(Guid buyerId, Guid orderId, CancellationToken ct = default);
    Task<Result> ProcessPaymentAsync(Guid buyerId, ProcessPaymentDto dto, CancellationToken ct = default);
    Task<Result> OpenDisputeAsync(Guid buyerId, Guid orderId, string reason, CancellationToken ct = default);
}
