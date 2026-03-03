using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Inspections;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Vehicle inspection report endpoints.
/// </summary>
[Authorize]
public class InspectionsController : BaseApiController
{
    private readonly IInspectionService _inspectionService;

    public InspectionsController(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInspectionDto dto, CancellationToken ct)
        => ToResponse(await _inspectionService.CreateAsync(GetCurrentUserId(), dto, ct));

    [AllowAnonymous]
    [HttpGet("listing/{listingId:int}")]
    public async Task<IActionResult> GetByListing(int listingId, CancellationToken ct)
        => ToResponse(await _inspectionService.GetByListingAsync(listingId, ct));

    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports(CancellationToken ct)
        => ToResponse(await _inspectionService.GetByInspectorAsync(GetCurrentUserId(), ct));

    [HttpPatch("{reportId:int}/complete")]
    public async Task<IActionResult> Complete(int reportId, CancellationToken ct)
        => ToResponse(await _inspectionService.CompleteAsync(GetCurrentUserId(), reportId, ct));
}
