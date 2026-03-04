using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Abuse;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for abuse report management.
/// Buyer submits RequestAbuse; Admin resolves via ReportAbuse.
/// </summary>
public interface IAbuseService
{
    Task<Result<AbuseRequestDto>> SubmitAsync(int reporterId, CreateAbuseRequestDto dto, CancellationToken ct = default);
    Task<Result<List<AbuseRequestDto>>> GetMyRequestsAsync(int userId, CancellationToken ct = default);
    Task<Result<List<AbuseRequestDto>>> GetPendingRequestsAsync(CancellationToken ct = default);
    Task<Result<List<AbuseReportDto>>> GetAllReportsAsync(CancellationToken ct = default);
    Task<Result<AbuseReportDto>> ResolveAsync(int adminId, ResolveAbuseRequestDto dto, CancellationToken ct = default);
}
