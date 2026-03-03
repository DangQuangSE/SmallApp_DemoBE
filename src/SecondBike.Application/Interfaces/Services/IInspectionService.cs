using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Inspections;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for vehicle inspections.
/// </summary>
public interface IInspectionService
{
    Task<Result<InspectionReportDto>> CreateAsync(int inspectorId, CreateInspectionDto dto, CancellationToken ct = default);
    Task<Result<InspectionReportDto>> GetByListingAsync(int listingId, CancellationToken ct = default);
    Task<Result<List<InspectionReportDto>>> GetByInspectorAsync(int inspectorId, CancellationToken ct = default);
    Task<Result> CompleteAsync(int inspectorId, int reportId, CancellationToken ct = default);
}
