using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Shopping cart endpoints for buyers.
/// </summary>
[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart(CancellationToken ct)
        => ToResponse(await _cartService.GetByUserAsync(GetCurrentUserId(), ct));

    [HttpPost("{listingId:int}")]
    public async Task<IActionResult> Add(int listingId, CancellationToken ct)
        => ToResponse(await _cartService.AddAsync(GetCurrentUserId(), listingId, ct));

    [HttpDelete("{listingId:int}")]
    public async Task<IActionResult> Remove(int listingId, CancellationToken ct)
        => ToResponse(await _cartService.RemoveAsync(GetCurrentUserId(), listingId, ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
        => ToResponse(await _cartService.ClearAsync(GetCurrentUserId(), ct));

    [HttpGet("{listingId:int}/check")]
    public async Task<IActionResult> Check(int listingId, CancellationToken ct)
        => ToResponse(await _cartService.IsInCartAsync(GetCurrentUserId(), listingId, ct));

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
        => ToResponse(await _cartService.GetCountAsync(GetCurrentUserId(), ct));
}
