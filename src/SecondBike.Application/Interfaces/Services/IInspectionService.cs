using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Inspections;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for vehicle inspections.
/// Seller creates request ? Inspector accepts ? Inspector uploads report ? completes.
/// </summary>
public interface IInspectionService
{
    // ===== Seller =====
    Task<Result<InspectionRequestDto>> CreateRequestAsync(int sellerId, CreateInspectionRequestDto dto, CancellationToken ct = default);
    Task<Result<List<InspectionRequestDto>>> GetMyRequestsAsync(int sellerId, CancellationToken ct = default);
    Task<Result> CancelRequestAsync(int sellerId, int requestId, CancellationToken ct = default);

    // ===== Inspector =====
    Task<Result<List<InspectionRequestDto>>> GetPendingRequestsAsync(CancellationToken ct = default);
    Task<Result<List<InspectionRequestDto>>> GetMyAssignedRequestsAsync(int inspectorId, CancellationToken ct = default);
    Task<Result> AcceptRequestAsync(int inspectorId, int requestId, CancellationToken ct = default);
    Task<Result<InspectionReportDto>> UploadReportAsync(int inspectorId, UploadInspectionReportDto dto, CancellationToken ct = default);
    Task<Result<List<InspectionReportDto>>> GetMyReportsAsync(int inspectorId, CancellationToken ct = default);

    // ===== Public =====
    Task<Result<InspectionReportDto>> GetByListingAsync(int listingId, CancellationToken ct = default);
    Task<Result<InspectionReportDto>> GetByRequestIdAsync(int requestId, CancellationToken ct = default);
}
