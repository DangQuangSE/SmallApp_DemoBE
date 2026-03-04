using SecondBike.Application.Common;
using SecondBike.Application.DTOs.UserManagement;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for managing users (Admin feature).
/// Provides CRUD operations with pagination and filtering.
/// </summary>
public interface IUserManagerService
{
    Task<Result<PagedResult<UserManagementDto>>> GetUsersAsync(UserFilterDto filter, CancellationToken ct = default);
    Task<Result<UserManagementDto>> GetByIdAsync(int userId, CancellationToken ct = default);
    Task<Result<UserManagementDto>> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Result<UserManagementDto>> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int userId, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(int userId, string newPassword, CancellationToken ct = default);
}
