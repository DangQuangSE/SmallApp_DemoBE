using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Admin;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Domain.Enums;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for admin operations (Admin Dashboard — Team Member 6).
/// </summary>
public interface IAdminService
{
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<Result<List<PendingPostDto>>> GetPendingPostsAsync(CancellationToken ct = default);
    Task<Result> ModeratePostAsync(Guid adminId, ModeratePostDto dto, CancellationToken ct = default);
    Task<Result<List<AdminUserDto>>> GetUsersAsync(UserRole? roleFilter = null, CancellationToken ct = default);
    Task<Result> UpdateUserStatusAsync(Guid adminId, Guid userId, UserStatus status, CancellationToken ct = default);
    Task<Result> ResolveDisputeAsync(Guid adminId, ResolveDisputeDto dto, CancellationToken ct = default);
    Task<Result<List<OrderDto>>> GetDisputedOrdersAsync(CancellationToken ct = default);
}
