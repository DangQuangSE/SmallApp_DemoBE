using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Endpoint for Admin to manage the bicycle catalogue.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminBicyclesController : BaseApiController
{
    private readonly IBicycleAdminService _bicycleAdminService;

    public AdminBicyclesController(IBicycleAdminService bicycleAdminService)
    {
        _bicycleAdminService = bicycleAdminService;
    }

    /// <summary>
    /// Creates a new generic Bicycle model. Returns the BikeId.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBicycleDto dto, CancellationToken ct)
        => ToResponse(await _bicycleAdminService.CreateBicycleAsync(dto, ct));
}
