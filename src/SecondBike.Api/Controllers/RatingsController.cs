using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Ratings;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Seller rating endpoints.
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
    [HttpGet("seller/{sellerId:guid}")]
    public async Task<IActionResult> GetBySeller(Guid sellerId, CancellationToken ct)
        => ToResponse(await _ratingService.GetBySellerAsync(sellerId, ct));

    [HttpPost("{ratingId:guid}/respond")]
    public async Task<IActionResult> Respond(Guid ratingId, [FromBody] RespondRequest request, CancellationToken ct)
        => ToResponse(await _ratingService.RespondAsync(GetCurrentUserId(), ratingId, request.Response, ct));
}

public class RespondRequest
{
    public string Response { get; set; } = string.Empty;
}
