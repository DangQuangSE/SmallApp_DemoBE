using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Inspections;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Quality & Auth — Vehicle inspection reports.
/// </summary>
public class InspectionService : IInspectionService
{
    private readonly IRepository<InspectionReport> _reportRepo;
    private readonly IRepository<BikePost> _postRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly IUnitOfWork _uow;

    public InspectionService(
        IRepository<InspectionReport> reportRepo,
        IRepository<BikePost> postRepo,
        IRepository<AppUser> userRepo,
        IUnitOfWork uow)
    {
        _reportRepo = reportRepo;
        _postRepo = postRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<InspectionReportDto>> CreateAsync(Guid inspectorId, CreateInspectionDto dto, CancellationToken ct = default)
    {
        var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);
        if (inspector is null || inspector.Role != UserRole.Inspector)
            return Result<InspectionReportDto>.Failure("Only inspectors can create reports");

        var post = await _postRepo.GetByIdAsync(dto.BikePostId, ct);
        if (post is null) return Result<InspectionReportDto>.Failure("Bike post not found");

        var existing = await _reportRepo.AnyAsync(r => r.BikePostId == dto.BikePostId, ct);
        if (existing) return Result<InspectionReportDto>.Failure("This bike already has an inspection report");

        var report = new InspectionReport
        {
            BikePostId = dto.BikePostId,
            InspectorId = inspectorId,
            ReportNumber = $"INS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Status = InspectionStatus.InProgress,
            OverallCondition = dto.OverallCondition,
            EstimatedValue = dto.EstimatedValue,
            IsRecommended = dto.IsRecommended,
            Summary = dto.Summary,
            FrameScore = dto.FrameScore,
            BrakesScore = dto.BrakesScore,
            GearsScore = dto.GearsScore,
            WheelsScore = dto.WheelsScore,
            TiresScore = dto.TiresScore,
            ChainScore = dto.ChainScore,
            HasFrameDamage = dto.HasFrameDamage,
            FrameNotes = dto.FrameNotes,
            HasRust = dto.HasRust,
            HasCracks = dto.HasCracks,
            AllComponentsOriginal = dto.AllComponentsOriginal,
            ReplacedComponents = dto.ReplacedComponents
        };

        await _reportRepo.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<InspectionReportDto>.Success(MapToDto(report, inspector, post));
    }

    public async Task<Result<InspectionReportDto>> GetByBikePostAsync(Guid bikePostId, CancellationToken ct = default)
    {
        var reports = await _reportRepo.FindAsync(r => r.BikePostId == bikePostId, ct);
        var report = reports.FirstOrDefault();
        if (report is null) return Result<InspectionReportDto>.Failure("No inspection report found");

        var inspector = await _userRepo.GetByIdAsync(report.InspectorId, ct);
        var post = await _postRepo.GetByIdAsync(bikePostId, ct);

        return Result<InspectionReportDto>.Success(MapToDto(report, inspector!, post!));
    }

    public async Task<Result<List<InspectionReportDto>>> GetByInspectorAsync(Guid inspectorId, CancellationToken ct = default)
    {
        var reports = await _reportRepo.FindAsync(r => r.InspectorId == inspectorId, ct);
        var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);

        var dtos = new List<InspectionReportDto>();
        foreach (var r in reports)
        {
            var post = await _postRepo.GetByIdAsync(r.BikePostId, ct);
            dtos.Add(MapToDto(r, inspector!, post!));
        }

        return Result<List<InspectionReportDto>>.Success(dtos);
    }

    public async Task<Result> CompleteAsync(Guid inspectorId, Guid reportId, CancellationToken ct = default)
    {
        var report = await _reportRepo.GetByIdAsync(reportId, ct);
        if (report is null) return Result.Failure("Report not found");
        if (report.InspectorId != inspectorId) return Result.Failure("Access denied");

        report.Status = InspectionStatus.Completed;
        report.InspectedAt = DateTime.UtcNow;
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static InspectionReportDto MapToDto(InspectionReport r, AppUser inspector, BikePost post)
    {
        return new InspectionReportDto
        {
            Id = r.Id,
            ReportNumber = r.ReportNumber,
            Status = r.Status,
            OverallCondition = r.OverallCondition,
            EstimatedValue = r.EstimatedValue,
            IsRecommended = r.IsRecommended,
            Summary = r.Summary,
            FrameScore = r.FrameScore,
            BrakesScore = r.BrakesScore,
            GearsScore = r.GearsScore,
            WheelsScore = r.WheelsScore,
            TiresScore = r.TiresScore,
            ChainScore = r.ChainScore,
            HasFrameDamage = r.HasFrameDamage,
            HasRust = r.HasRust,
            HasCracks = r.HasCracks,
            InspectorName = inspector.FullName,
            InspectedAt = r.InspectedAt,
            BikeTitle = post.Title
        };
    }
}
