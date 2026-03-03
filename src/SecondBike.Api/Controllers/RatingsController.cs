using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Ratings;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Seller rating/feedback endpoints.
/// </summary>
[Authorize]
public class RatingsController : BaseApiController
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRatingDto dto, CancellationToken ct)
        => ToResponse(await _ratingService.CreateAsync(GetCurrentUserId(), dto, ct));

    [AllowAnonymous]
    [HttpGet("seller/{sellerId:int}")]
    public async Task<IActionResult> GetBySeller(int sellerId, CancellationToken ct)
        => ToResponse(await _ratingService.GetBySellerAsync(sellerId, ct));

    [AllowAnonymous]
    [HttpGet("seller/{sellerId:int}/stats")]
    public async Task<IActionResult> GetSellerStats(int sellerId, CancellationToken ct)
        => ToResponse(await _ratingService.GetSellerStatsAsync(sellerId, ct));

    [HttpGet("order/{orderId:int}/check")]
    public async Task<IActionResult> HasRatedOrder(int orderId, CancellationToken ct)
        => ToResponse(await _ratingService.HasRatedOrderAsync(GetCurrentUserId(), orderId, ct));
}
