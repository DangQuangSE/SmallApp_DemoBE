using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Orders;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for orders and payments (Transaction Hub).
/// </summary>
public interface IOrderService
{
    Task<Result<OrderDto>> PlaceOrderAsync(int buyerId, CreateOrderDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> GetByIdAsync(int orderId, CancellationToken ct = default);
    Task<Result<List<OrderDto>>> GetByBuyerAsync(int buyerId, CancellationToken ct = default);
    Task<Result> CancelOrderAsync(int userId, int orderId, CancellationToken ct = default);
    Task<Result> ConfirmDeliveryAsync(int buyerId, int orderId, CancellationToken ct = default);
    Task<Result> ProcessPaymentAsync(int buyerId, ProcessPaymentDto dto, CancellationToken ct = default);
}
