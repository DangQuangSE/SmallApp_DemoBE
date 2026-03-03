using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// User profile management endpoints.
/// </summary>
[Authorize]
public class ProfileController : BaseApiController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Get the current user's profile.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
        => ToResponse(await _profileService.GetProfileAsync(GetCurrentUserId(), ct));

    /// <summary>
    /// Update the current user's profile (fullName, phoneNumber, address).
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
        => ToResponse(await _profileService.UpdateProfileAsync(GetCurrentUserId(), dto, ct));

    /// <summary>
    /// Upload or replace the current user's avatar image.
    /// </summary>
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "File size must not exceed 5 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Only JPEG, PNG, and WebP images are allowed" });

        using var stream = file.OpenReadStream();
        return ToResponse(await _profileService.UpdateAvatarAsync(GetCurrentUserId(), stream, file.FileName, ct));
    }

    /// <summary>
    /// Remove the current user's avatar image.
    /// </summary>
    [HttpDelete("avatar")]
    public async Task<IActionResult> RemoveAvatar(CancellationToken ct)
        => ToResponse(await _profileService.RemoveAvatarAsync(GetCurrentUserId(), ct));

    /// <summary>
    /// Change the current user's password.
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
        => ToResponse(await _profileService.ChangePasswordAsync(GetCurrentUserId(), dto, ct));
}
