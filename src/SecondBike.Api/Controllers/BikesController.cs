using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Bike search &amp; browsing endpoints (public) + seller listing management (authenticated).
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

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] BikeFilterDto filter, CancellationToken ct)
        => ToResponse(await _bikeSearchService.SearchAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
        => ToResponse(await _bikeSearchService.GetDetailAsync(id, ct));

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken ct)
        => ToResponse(await _bikeSearchService.GetBrandsAsync(ct));

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes(CancellationToken ct)
        => ToResponse(await _bikeSearchService.GetTypesAsync(ct));

    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateBikePostDto dto, CancellationToken ct)
        => ToResponse(await _bikePostService.CreateAsync(GetCurrentUserId(), dto, ct));

    [Authorize]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] UpdateBikePostDto dto, CancellationToken ct)
        => ToResponse(await _bikePostService.UpdateAsync(GetCurrentUserId(), dto, ct));

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => ToResponse(await _bikePostService.DeleteAsync(GetCurrentUserId(), id, ct));

    [Authorize]
    [HttpPatch("{id:int}/visibility")]
    public async Task<IActionResult> ToggleVisibility(int id, CancellationToken ct)
        => ToResponse(await _bikePostService.ToggleVisibilityAsync(GetCurrentUserId(), id, ct));

    [Authorize]
    [HttpGet("my-posts")]
    public async Task<IActionResult> GetMyPosts(CancellationToken ct)
        => ToResponse(await _bikePostService.GetBySellerAsync(GetCurrentUserId(), ct));
}
