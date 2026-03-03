using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Authentication endpoints — register, login, OTP verification.
/// </summary>
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user. An OTP code will be sent via email.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        => ToResponse(await _authService.RegisterAsync(dto, ct));

    /// <summary>
    /// Login with email and password. Requires verified email.
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
    /// Verify email using the 6-digit OTP code sent to the user's email.
    /// </summary>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(dto.Email, dto.Otp, ct);
        if (result.IsSuccess)
            return Ok(new { message = "Email confirmed successfully!" });
        return BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Resend OTP verification code to the specified email.
    /// </summary>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto dto, CancellationToken ct)
        => ToResponse(await _authService.ResendConfirmationEmailAsync(dto.Email, ct));

    /// <summary>
    /// Logout the current user.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { message = "Logged out successfully" });
    }
}
