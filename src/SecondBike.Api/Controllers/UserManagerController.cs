using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.UserManagement;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// User management endpoints for admin - CRUD, search, reset password.
/// </summary>
[Authorize(Roles = "Admin")]
public class UserManagerController : BaseApiController
{
    private readonly IUserManagerService _userManagerService;

    public UserManagerController(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter, CancellationToken ct)
        => ToResponse(await _userManagerService.GetUsersAsync(filter, ct));

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetById(int userId, CancellationToken ct)
        => ToResponse(await _userManagerService.GetByIdAsync(userId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
        => ToResponse(await _userManagerService.CreateAsync(dto, ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto, CancellationToken ct)
        => ToResponse(await _userManagerService.UpdateAsync(dto, ct));

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Delete(int userId, CancellationToken ct)
        => ToResponse(await _userManagerService.DeleteAsync(userId, ct));

    [HttpPost("{userId:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto request, CancellationToken ct)
        => ToResponse(await _userManagerService.ResetPasswordAsync(userId, request.NewPassword, ct));
}