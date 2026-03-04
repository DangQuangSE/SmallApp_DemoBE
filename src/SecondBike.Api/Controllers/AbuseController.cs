using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Abuse;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Abuse report endpoints — buyers submit reports, view their own reports.
/// </summary>
[Authorize]
public class AbuseController : BaseApiController
{
    private readonly IAbuseService _abuseService;

    public AbuseController(IAbuseService abuseService)
    {
        _abuseService = abuseService;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateAbuseRequestDto dto, CancellationToken ct)
        => ToResponse(await _abuseService.SubmitAsync(GetCurrentUserId(), dto, ct));

    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyRequests(CancellationToken ct)
        => ToResponse(await _abuseService.GetMyRequestsAsync(GetCurrentUserId(), ct));
}
