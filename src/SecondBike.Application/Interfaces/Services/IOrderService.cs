using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Orders;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for orders, deposits, and VNPay payment processing (Transaction Hub).
/// </summary>
public interface IOrderService
{
    Task<Result<OrderDto>> PlaceOrderAsync(int buyerId, CreateOrderDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> GetByIdAsync(int userId, int orderId, CancellationToken ct = default);
    Task<Result<List<OrderDto>>> GetByBuyerAsync(int buyerId, CancellationToken ct = default);
    Task<Result> CancelOrderAsync(int userId, int orderId, CancellationToken ct = default);
    Task<Result> ConfirmDeliveryAsync(int buyerId, int orderId, CancellationToken ct = default);
    Task<Result<PaymentUrlResultDto>> CreatePaymentUrlAsync(int buyerId, CreatePaymentUrlDto dto, string clientIp, CancellationToken ct = default);
    Task<Result<OrderDto>> ProcessVnPayCallbackAsync(IDictionary<string, string> queryParams, CancellationToken ct = default);
}
