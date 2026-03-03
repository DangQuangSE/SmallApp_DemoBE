using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Wishlist (favorite bikes) endpoints.
/// </summary>
[Authorize]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWishlist(CancellationToken ct)
        => ToResponse(await _wishlistService.GetByUserAsync(GetCurrentUserId(), ct));

    [HttpPost("{listingId:int}")]
    public async Task<IActionResult> Add(int listingId, CancellationToken ct)
        => ToResponse(await _wishlistService.AddAsync(GetCurrentUserId(), listingId, ct));

    [HttpDelete("{listingId:int}")]
    public async Task<IActionResult> Remove(int listingId, CancellationToken ct)
        => ToResponse(await _wishlistService.RemoveAsync(GetCurrentUserId(), listingId, ct));

    [HttpGet("{listingId:int}/check")]
    public async Task<IActionResult> Check(int listingId, CancellationToken ct)
        => ToResponse(await _wishlistService.IsInWishlistAsync(GetCurrentUserId(), listingId, ct));
}
