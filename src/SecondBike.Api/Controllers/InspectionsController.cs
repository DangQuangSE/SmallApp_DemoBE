using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Inspections;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Vehicle inspection endpoints.
/// Seller creates request ? Inspector accepts ? Inspector uploads report.
/// </summary>
[Authorize]
public class InspectionsController : BaseApiController
{
    private readonly IInspectionService _inspectionService;

    public InspectionsController(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    // ===== Seller Endpoints =====

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateInspectionRequestDto dto, CancellationToken ct)
        => ToResponse(await _inspectionService.CreateRequestAsync(GetCurrentUserId(), dto, ct));

    [HttpGet("requests/my")]
    public async Task<IActionResult> GetMyRequests(CancellationToken ct)
        => ToResponse(await _inspectionService.GetMyRequestsAsync(GetCurrentUserId(), ct));

    [HttpDelete("requests/{requestId:int}")]
    public async Task<IActionResult> CancelRequest(int requestId, CancellationToken ct)
        => ToResponse(await _inspectionService.CancelRequestAsync(GetCurrentUserId(), requestId, ct));

    // ===== Inspector Endpoints =====

    [Authorize(Roles = "Inspector")]
    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests(CancellationToken ct)
        => ToResponse(await _inspectionService.GetPendingRequestsAsync(ct));

    [Authorize(Roles = "Inspector")]
    [HttpGet("requests/assigned")]
    public async Task<IActionResult> GetMyAssignedRequests(CancellationToken ct)
        => ToResponse(await _inspectionService.GetMyAssignedRequestsAsync(GetCurrentUserId(), ct));

    [Authorize(Roles = "Inspector")]
    [HttpPatch("requests/{requestId:int}/accept")]
    public async Task<IActionResult> AcceptRequest(int requestId, CancellationToken ct)
        => ToResponse(await _inspectionService.AcceptRequestAsync(GetCurrentUserId(), requestId, ct));

    [Authorize(Roles = "Inspector")]
    [HttpPost("reports")]
    public async Task<IActionResult> UploadReport([FromBody] UploadInspectionReportDto dto, CancellationToken ct)
        => ToResponse(await _inspectionService.UploadReportAsync(GetCurrentUserId(), dto, ct));

    [Authorize(Roles = "Inspector")]
    [HttpGet("reports/my")]
    public async Task<IActionResult> GetMyReports(CancellationToken ct)
        => ToResponse(await _inspectionService.GetMyReportsAsync(GetCurrentUserId(), ct));

    // ===== Public Endpoints =====

    [AllowAnonymous]
    [HttpGet("listing/{listingId:int}")]
    public async Task<IActionResult> GetByListing(int listingId, CancellationToken ct)
        => ToResponse(await _inspectionService.GetByListingAsync(listingId, ct));

    [AllowAnonymous]
    [HttpGet("reports/{requestId:int}")]
    public async Task<IActionResult> GetByRequestId(int requestId, CancellationToken ct)
        => ToResponse(await _inspectionService.GetByRequestIdAsync(requestId, ct));
}
