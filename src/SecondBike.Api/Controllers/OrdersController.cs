using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Order placement, deposit, and VNPay payment processing endpoints.
/// </summary>
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Place a new order for a listing. Creates a 20% deposit record automatically.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
        => ToResponse(await _orderService.PlaceOrderAsync(GetCurrentUserId(), dto, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToResponse(await _orderService.GetByIdAsync(GetCurrentUserId(), id, ct));

    [HttpGet("my-purchases")]
    public async Task<IActionResult> GetMyPurchases(CancellationToken ct)
        => ToResponse(await _orderService.GetByBuyerAsync(GetCurrentUserId(), ct));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        => ToResponse(await _orderService.CancelOrderAsync(GetCurrentUserId(), id, ct));

    [HttpPost("{id:int}/confirm-delivery")]
    public async Task<IActionResult> ConfirmDelivery(int id, CancellationToken ct)
        => ToResponse(await _orderService.ConfirmDeliveryAsync(GetCurrentUserId(), id, ct));

    /// <summary>
    /// Create a VNPay payment URL for deposit (20%) or full remaining payment (80%).
    /// FE should redirect the user to the returned URL.
    /// </summary>
    [HttpPost("create-payment-url")]
    public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentUrlDto dto, CancellationToken ct)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
        return ToResponse(await _orderService.CreatePaymentUrlAsync(GetCurrentUserId(), dto, clientIp, ct));
    }

    /// <summary>
    /// VNPay return URL — browser redirects here after payment.
    /// Validates the callback, updates order status, and redirects FE to order detail page.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("vnpay-return")]
    public async Task<IActionResult> VnPayReturn(CancellationToken ct)
    {
        var queryParams = HttpContext.Request.Query
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        var result = await _orderService.ProcessVnPayCallbackAsync(queryParams, ct);

        // Redirect to FE order page regardless of result
        var feBaseUrl = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["App:BaseUrl"] ?? "http://localhost:5174";

        if (result.IsSuccess)
        {
            var orderId = result.Data!.OrderId;
            return Redirect($"{feBaseUrl}/orders/{orderId}?payment=success");
        }

        return Redirect($"{feBaseUrl}/orders?payment=failed&error={Uri.EscapeDataString(result.ErrorMessage ?? "Payment failed")}");
    }

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) — server-to-server callback.
    /// VNPay calls this endpoint directly (no user interaction).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("vnpay-ipn")]
    public async Task<IActionResult> VnPayIpn(CancellationToken ct)
    {
        var queryParams = HttpContext.Request.Query
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        var result = await _orderService.ProcessVnPayCallbackAsync(queryParams, ct);

        // VNPay expects this exact JSON response format
        if (result.IsSuccess)
            return Ok(new { RspCode = "00", Message = "Confirm Success" });

        return Ok(new { RspCode = "99", Message = result.ErrorMessage ?? "Unknown error" });
    }
}
