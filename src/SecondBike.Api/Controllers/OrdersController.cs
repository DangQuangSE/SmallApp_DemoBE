using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Order placement, payment processing endpoints.
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToResponse(await _orderService.GetByIdAsync(id, ct));

    [HttpGet("my-purchases")]
    public async Task<IActionResult> GetMyPurchases(CancellationToken ct)
        => ToResponse(await _orderService.GetByBuyerAsync(GetCurrentUserId(), ct));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        => ToResponse(await _orderService.CancelOrderAsync(GetCurrentUserId(), id, ct));

    [HttpPost("{id:int}/confirm-delivery")]
    public async Task<IActionResult> ConfirmDelivery(int id, CancellationToken ct)
        => ToResponse(await _orderService.ConfirmDeliveryAsync(GetCurrentUserId(), id, ct));

    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto, CancellationToken ct)
        => ToResponse(await _orderService.ProcessPaymentAsync(GetCurrentUserId(), dto, ct));
}
