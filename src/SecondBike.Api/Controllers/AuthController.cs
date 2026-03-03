using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Authentication, registration, and profile management endpoints.
/// </summary>
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user. A confirmation email will be sent.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        => ToResponse(await _authService.RegisterAsync(dto, ct));

    /// <summary>
    /// Login with email and password. Requires confirmed email.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => ToResponse(await _authService.LoginAsync(dto, ct));

    /// <summary>
    /// Login / register via Google OAuth.
    /// </summary>
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto, CancellationToken ct)
        => ToResponse(await _authService.GoogleLoginAsync(dto, ct));

    /// <summary>
    /// Confirm email address using the token sent via email.
    /// </summary>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(email, token, ct);
        if (result.IsSuccess)
            return Ok(new { message = "Email confirmed successfully!" });
        return BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Resend confirmation email to the specified address.
    /// </summary>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto dto, CancellationToken ct)
        => ToResponse(await _authService.ResendConfirmationEmailAsync(dto.Email, ct));

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
        => ToResponse(await _authService.GetProfileAsync(GetCurrentUserId(), ct));

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
        => ToResponse(await _authService.UpdateProfileAsync(GetCurrentUserId(), dto, ct));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { message = "Logged out successfully" });
    }
}
