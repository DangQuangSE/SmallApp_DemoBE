using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Endpoint for managing the bicycle catalogue.
/// </summary>
public class AdminBicyclesController : BaseApiController
{
    private readonly IBicycleAdminService _bicycleAdminService;

    public AdminBicyclesController(IBicycleAdminService bicycleAdminService)
    {
        _bicycleAdminService = bicycleAdminService;
    }

    /// <summary>
    /// Creates a new generic Bicycle model. Returns the BikeId.
    /// Only Admin can create bikes.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBicycleDto dto, CancellationToken ct)
        => ToResponse(await _bicycleAdminService.CreateBicycleAsync(dto, ct));

    /// <summary>
    /// Gets all generic Bicycle models for dropdowns and dashboard.
    /// Accessible by any authenticated user so sellers can populate their CreateListing dropdown.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => ToResponse(await _bicycleAdminService.GetAllBicyclesAsync(ct));
}
