using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Admin;
using SecondBike.Application.DTOs.Orders;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for admin operations (Admin Dashboard).
/// </summary>
public interface IAdminService
{
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<Result<List<PendingPostDto>>> GetPendingPostsAsync(CancellationToken ct = default);
    Task<Result> ModeratePostAsync(int adminId, ModeratePostDto dto, CancellationToken ct = default);
    Task<Result<List<AdminUserDto>>> GetUsersAsync(int? roleId = null, CancellationToken ct = default);
    Task<Result> UpdateUserStatusAsync(int adminId, int userId, byte status, CancellationToken ct = default);
    Task<Result> ResolveDisputeAsync(int adminId, ResolveDisputeDto dto, CancellationToken ct = default);
}
