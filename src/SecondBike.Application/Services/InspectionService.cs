using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Inspections;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Vehicle inspection reports using InspectionRequest/InspectionReport entities.
/// Business logic belongs in Application layer.
/// </summary>
public class InspectionService : IInspectionService
{
    private readonly IRepository<InspectionRequest> _requestRepo;
    private readonly IRepository<InspectionReport> _reportRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public InspectionService(
        IRepository<InspectionRequest> requestRepo,
        IRepository<InspectionReport> reportRepo,
        IRepository<BicycleListing> listingRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _requestRepo = requestRepo;
        _reportRepo = reportRepo;
        _listingRepo = listingRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<InspectionReportDto>> CreateAsync(int inspectorId, CreateInspectionDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null) return Result<InspectionReportDto>.Failure("Listing not found");

        var request = new InspectionRequest
        {
            ListingId = dto.ListingId,
            InspectorId = inspectorId,
            RequestStatus = 2,
            RequestDate = DateTime.UtcNow
        };
        await _requestRepo.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        var report = new InspectionReport
        {
            RequestId = request.RequestId,
            FrameCheck = dto.FrameCheck,
            BrakeCheck = dto.BrakeCheck,
            TransmissionCheck = dto.TransmissionCheck,
            InspectorNote = dto.InspectorNote,
            FinalVerdict = dto.FinalVerdict,
            ReportUrl = dto.ReportUrl,
            CompletedAt = DateTime.UtcNow
        };
        await _reportRepo.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);

        var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);
        return Result<InspectionReportDto>.Success(MapToDto(report, request, inspector, listing));
    }

    public async Task<Result<InspectionReportDto>> GetByListingAsync(int listingId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindAsync(r => r.ListingId == listingId, ct);
        var request = requests.FirstOrDefault();
        if (request is null) return Result<InspectionReportDto>.Failure("No inspection found");

        var reports = await _reportRepo.FindAsync(r => r.RequestId == request.RequestId, ct);
        var report = reports.FirstOrDefault();
        if (report is null) return Result<InspectionReportDto>.Failure("No inspection report found");

        var inspector = request.InspectorId.HasValue
            ? await _userRepo.GetByIdAsync(request.InspectorId.Value, ct)
            : null;
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);

        return Result<InspectionReportDto>.Success(MapToDto(report, request, inspector, listing));
    }

    public async Task<Result<List<InspectionReportDto>>> GetByInspectorAsync(int inspectorId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindAsync(r => r.InspectorId == inspectorId, ct);
        var dtos = new List<InspectionReportDto>();

        foreach (var request in requests)
        {
            var reports = await _reportRepo.FindAsync(r => r.RequestId == request.RequestId, ct);
            var report = reports.FirstOrDefault();
            if (report is null) continue;

            var listing = await _listingRepo.GetByIdAsync(request.ListingId, ct);
            var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);
            dtos.Add(MapToDto(report, request, inspector, listing));
        }

        return Result<List<InspectionReportDto>>.Success(dtos);
    }

    public async Task<Result> CompleteAsync(int inspectorId, int reportId, CancellationToken ct = default)
    {
        var report = await _reportRepo.GetByIdAsync(reportId, ct);
        if (report is null) return Result.Failure("Report not found");

        var request = await _requestRepo.GetByIdAsync(report.RequestId, ct);
        if (request is null) return Result.Failure("Request not found");
        if (request.InspectorId != inspectorId) return Result.Failure("Access denied");

        request.RequestStatus = 3;
        report.CompletedAt = DateTime.UtcNow;

        _requestRepo.Update(request);
        _reportRepo.Update(report);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static InspectionReportDto MapToDto(InspectionReport report, InspectionRequest request, User? inspector, BicycleListing? listing)
    {
        return new InspectionReportDto
        {
            ReportId = report.ReportId,
            RequestId = report.RequestId,
            RequestStatus = request.RequestStatus,
            FrameCheck = report.FrameCheck,
            BrakeCheck = report.BrakeCheck,
            TransmissionCheck = report.TransmissionCheck,
            InspectorNote = report.InspectorNote,
            FinalVerdict = report.FinalVerdict,
            ReportUrl = report.ReportUrl,
            CompletedAt = report.CompletedAt,
            InspectorName = inspector?.Username ?? "Unknown",
            BikeTitle = listing?.Title ?? "Unknown"
        };
    }
}
