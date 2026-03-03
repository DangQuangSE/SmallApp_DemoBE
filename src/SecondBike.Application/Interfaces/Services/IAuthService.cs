using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for authentication and profile management.
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<AuthResultDto>> GoogleLoginAsync(GoogleLoginDto dto, CancellationToken ct = default);
    Task<Result<UserProfileDto>> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto, CancellationToken ct = default);
    Task<Result> ConfirmEmailAsync(string email, string token, CancellationToken ct = default);
    Task<Result> ResendConfirmationEmailAsync(string email, CancellationToken ct = default);
    Task LogoutAsync();
}
