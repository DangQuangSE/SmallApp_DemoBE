using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for user profile management (SRP: separated from authentication).
/// </summary>
public interface IProfileService
{
    Task<Result<UserProfileDto>> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateAvatarAsync(int userId, Stream imageStream, string fileName, CancellationToken ct = default);
    Task<Result<UserProfileDto>> RemoveAvatarAsync(int userId, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default);
}
