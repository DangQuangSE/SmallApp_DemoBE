using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Admin;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Admin dashboard endpoints — listing moderation, user management, dispute resolution.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => ToResponse(await _adminService.GetDashboardStatsAsync(ct));

    [HttpGet("posts/pending")]
    public async Task<IActionResult> GetPendingPosts(CancellationToken ct)
        => ToResponse(await _adminService.GetPendingPostsAsync(ct));

    [HttpPost("posts/moderate")]
    public async Task<IActionResult> ModeratePost([FromBody] ModeratePostDto dto, CancellationToken ct)
        => ToResponse(await _adminService.ModeratePostAsync(GetCurrentUserId(), dto, ct));

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int? roleId, CancellationToken ct)
        => ToResponse(await _adminService.GetUsersAsync(roleId, ct));

    [HttpPatch("users/{userId:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await _adminService.UpdateUserStatusAsync(GetCurrentUserId(), userId, request.Status, ct));

    [HttpPost("disputes/resolve")]
    public async Task<IActionResult> ResolveDispute([FromBody] ResolveDisputeDto dto, CancellationToken ct)
        => ToResponse(await _adminService.ResolveDisputeAsync(GetCurrentUserId(), dto, ct));
}

public class UpdateStatusRequest
{
    public byte Status { get; set; }
}
