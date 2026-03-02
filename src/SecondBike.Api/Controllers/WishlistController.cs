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

    [HttpPost("{bikePostId:guid}")]
    public async Task<IActionResult> Add(Guid bikePostId, CancellationToken ct)
        => ToResponse(await _wishlistService.AddAsync(GetCurrentUserId(), bikePostId, ct));

    [HttpDelete("{bikePostId:guid}")]
    public async Task<IActionResult> Remove(Guid bikePostId, CancellationToken ct)
        => ToResponse(await _wishlistService.RemoveAsync(GetCurrentUserId(), bikePostId, ct));

    [HttpGet("{bikePostId:guid}/check")]
    public async Task<IActionResult> Check(Guid bikePostId, CancellationToken ct)
        => ToResponse(await _wishlistService.IsInWishlistAsync(GetCurrentUserId(), bikePostId, ct));
}
