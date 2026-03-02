using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Inspections;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for vehicle inspections (Quality & Auth — Team Member 5).
/// </summary>
public interface IInspectionService
{
    Task<Result<InspectionReportDto>> CreateAsync(Guid inspectorId, CreateInspectionDto dto, CancellationToken ct = default);
    Task<Result<InspectionReportDto>> GetByBikePostAsync(Guid bikePostId, CancellationToken ct = default);
    Task<Result<List<InspectionReportDto>>> GetByInspectorAsync(Guid inspectorId, CancellationToken ct = default);
    Task<Result> CompleteAsync(Guid inspectorId, Guid reportId, CancellationToken ct = default);
}
