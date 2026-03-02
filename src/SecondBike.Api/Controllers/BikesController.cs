using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Bike search &amp; browsing endpoints (public) + seller post management (authenticated).
/// </summary>
public class BikesController : BaseApiController
{
    private readonly IBikeSearchService _bikeSearchService;
    private readonly IBikePostService _bikePostService;

    public BikesController(IBikeSearchService bikeSearchService, IBikePostService bikePostService)
    {
        _bikeSearchService = bikeSearchService;
        _bikePostService = bikePostService;
    }

    // ????? Public Endpoints (Buyer Experience) ?????

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] BikeFilterDto filter, CancellationToken ct)
        => ToResponse(await _bikeSearchService.SearchAsync(filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
        => ToResponse(await _bikeSearchService.GetDetailAsync(id, ct));

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken ct)
        => ToResponse(await _bikeSearchService.GetBrandsAsync(ct));

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities(CancellationToken ct)
        => ToResponse(await _bikeSearchService.GetCitiesAsync(ct));

    // ????? Seller Endpoints (Seller Core) ?????

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBikePostDto dto, CancellationToken ct)
        => ToResponse(await _bikePostService.CreateAsync(GetCurrentUserId(), dto, ct));

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBikePostDto dto, CancellationToken ct)
        => ToResponse(await _bikePostService.UpdateAsync(GetCurrentUserId(), dto, ct));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => ToResponse(await _bikePostService.DeleteAsync(GetCurrentUserId(), id, ct));

    [Authorize]
    [HttpPatch("{id:guid}/visibility")]
    public async Task<IActionResult> ToggleVisibility(Guid id, CancellationToken ct)
        => ToResponse(await _bikePostService.ToggleVisibilityAsync(GetCurrentUserId(), id, ct));

    [Authorize]
    [HttpPatch("{id:guid}/submit")]
    public async Task<IActionResult> SubmitForModeration(Guid id, CancellationToken ct)
        => ToResponse(await _bikePostService.SubmitForModerationAsync(GetCurrentUserId(), id, ct));

    [Authorize]
    [HttpGet("my-posts")]
    public async Task<IActionResult> GetMyPosts(CancellationToken ct)
        => ToResponse(await _bikePostService.GetBySellerAsync(GetCurrentUserId(), ct));
}
