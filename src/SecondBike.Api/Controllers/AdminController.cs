using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Abuse;
using SecondBike.Application.DTOs.Admin;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Admin dashboard endpoints — listing moderation, user management, dispute resolution, abuse management.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly IAbuseService _abuseService;

    public AdminController(IAdminService adminService, IAbuseService abuseService)
    {
        _adminService = adminService;
        _abuseService = abuseService;
    }

    // ===== Dashboard =====

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => ToResponse(await _adminService.GetDashboardStatsAsync(ct));

    // ===== Post Moderation =====

    [HttpGet("posts/pending")]
    public async Task<IActionResult> GetPendingPosts(CancellationToken ct)
        => ToResponse(await _adminService.GetPendingPostsAsync(ct));

    [HttpPost("posts/moderate")]
    public async Task<IActionResult> ModeratePost([FromBody] ModeratePostDto dto, CancellationToken ct)
        => ToResponse(await _adminService.ModeratePostAsync(GetCurrentUserId(), dto, ct));

    // ===== User Management =====

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int? roleId, CancellationToken ct)
        => ToResponse(await _adminService.GetUsersAsync(roleId, ct));

    [HttpPatch("users/{userId:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await _adminService.UpdateUserStatusAsync(GetCurrentUserId(), userId, request.Status, ct));

    // ===== Dispute Resolution =====

    [HttpPost("disputes/resolve")]
    public async Task<IActionResult> ResolveDispute([FromBody] ResolveDisputeDto dto, CancellationToken ct)
        => ToResponse(await _adminService.ResolveDisputeAsync(GetCurrentUserId(), dto, ct));

    // ===== Abuse Management =====

    [HttpGet("abuse/pending")]
    public async Task<IActionResult> GetPendingAbuse(CancellationToken ct)
        => ToResponse(await _abuseService.GetPendingRequestsAsync(ct));

    [HttpGet("abuse/reports")]
    public async Task<IActionResult> GetAbuseReports(CancellationToken ct)
        => ToResponse(await _abuseService.GetAllReportsAsync(ct));

    [HttpPost("abuse/resolve")]
    public async Task<IActionResult> ResolveAbuse([FromBody] ResolveAbuseRequestDto dto, CancellationToken ct)
        => ToResponse(await _abuseService.ResolveAsync(GetCurrentUserId(), dto, ct));
}

public class UpdateStatusRequest
{
    public byte Status { get; set; }
}
