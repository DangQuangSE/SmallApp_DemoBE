using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for authentication and profile management (Quality & Auth — Team Member 5).
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default);
}
