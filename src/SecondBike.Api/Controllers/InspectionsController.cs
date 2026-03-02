using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Inspections;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Vehicle inspection report endpoints (Inspector role).
/// </summary>
[Authorize]
public class InspectionsController : BaseApiController
{
    private readonly IInspectionService _inspectionService;

    public InspectionsController(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    [Authorize(Roles = "Inspector")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInspectionDto dto, CancellationToken ct)
        => ToResponse(await _inspectionService.CreateAsync(GetCurrentUserId(), dto, ct));

    [AllowAnonymous]
    [HttpGet("bike/{bikePostId:guid}")]
    public async Task<IActionResult> GetByBikePost(Guid bikePostId, CancellationToken ct)
        => ToResponse(await _inspectionService.GetByBikePostAsync(bikePostId, ct));

    [Authorize(Roles = "Inspector")]
    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports(CancellationToken ct)
        => ToResponse(await _inspectionService.GetByInspectorAsync(GetCurrentUserId(), ct));

    [Authorize(Roles = "Inspector")]
    [HttpPatch("{reportId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid reportId, CancellationToken ct)
        => ToResponse(await _inspectionService.CompleteAsync(GetCurrentUserId(), reportId, ct));
}
