using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Order placement, payment processing, and dispute endpoints.
/// </summary>
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
        => ToResponse(await _orderService.PlaceOrderAsync(GetCurrentUserId(), dto, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => ToResponse(await _orderService.GetByIdAsync(id, ct));

    [HttpGet("my-purchases")]
    public async Task<IActionResult> GetMyPurchases(CancellationToken ct)
        => ToResponse(await _orderService.GetByBuyerAsync(GetCurrentUserId(), ct));

    [HttpGet("my-sales")]
    public async Task<IActionResult> GetMySales(CancellationToken ct)
        => ToResponse(await _orderService.GetBySellerAsync(GetCurrentUserId(), ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest request, CancellationToken ct)
        => ToResponse(await _orderService.CancelOrderAsync(GetCurrentUserId(), id, request.Reason, ct));

    [HttpPost("{id:guid}/confirm-delivery")]
    public async Task<IActionResult> ConfirmDelivery(Guid id, CancellationToken ct)
        => ToResponse(await _orderService.ConfirmDeliveryAsync(GetCurrentUserId(), id, ct));

    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto, CancellationToken ct)
        => ToResponse(await _orderService.ProcessPaymentAsync(GetCurrentUserId(), dto, ct));

    [HttpPost("{id:guid}/dispute")]
    public async Task<IActionResult> OpenDispute(Guid id, [FromBody] DisputeRequest request, CancellationToken ct)
        => ToResponse(await _orderService.OpenDisputeAsync(GetCurrentUserId(), id, request.Reason, ct));
}

public class CancelRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class DisputeRequest
{
    public string Reason { get; set; } = string.Empty;
}
