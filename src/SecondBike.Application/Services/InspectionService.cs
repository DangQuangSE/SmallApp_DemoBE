using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Inspections;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Vehicle inspection flow:
/// 1. Seller creates InspectionRequest (status=1 Pending)
/// 2. Inspector accepts request (status=2 InProgress, assigned InspectorId)
/// 3. Inspector uploads InspectionReport with findings
/// 4. Request auto-completes (status=3 Completed)
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

    // ===== Seller =====

    public async Task<Result<InspectionRequestDto>> CreateRequestAsync(int sellerId, CreateInspectionRequestDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null)
            return Result<InspectionRequestDto>.Failure("Listing not found");

        if (listing.SellerId != sellerId)
            return Result<InspectionRequestDto>.Failure("You can only request inspection for your own listing");

        if (listing.ListingStatus is not 1 and not 2)
            return Result<InspectionRequestDto>.Failure("Listing must be Active or Pending to request inspection");

        var existingPending = await _requestRepo.AnyAsync(r =>
            r.ListingId == dto.ListingId && (r.RequestStatus == 1 || r.RequestStatus == 2), ct);
        if (existingPending)
            return Result<InspectionRequestDto>.Failure("An inspection request is already pending or in progress for this listing");

        var request = new InspectionRequest
        {
            ListingId = dto.ListingId,
            InspectorId = null,
            RequestStatus = 1, // Pending
            RequestDate = DateTime.UtcNow
        };

        await _requestRepo.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<InspectionRequestDto>.Success(await MapToRequestDtoAsync(request, ct));
    }

    public async Task<Result<List<InspectionRequestDto>>> GetMyRequestsAsync(int sellerId, CancellationToken ct = default)
    {
        var listings = await _listingRepo.FindAsync(l => l.SellerId == sellerId, ct);
        var listingIds = listings.Select(l => l.ListingId).ToList();

        if (listingIds.Count == 0)
            return Result<List<InspectionRequestDto>>.Success(new List<InspectionRequestDto>());

        var allRequests = new List<InspectionRequest>();
        foreach (var lid in listingIds)
        {
            var reqs = await _requestRepo.FindAsync(r => r.ListingId == lid, ct);
            allRequests.AddRange(reqs);
        }

        var dtos = new List<InspectionRequestDto>();
        foreach (var req in allRequests.OrderByDescending(r => r.RequestDate))
            dtos.Add(await MapToRequestDtoAsync(req, ct));

        return Result<List<InspectionRequestDto>>.Success(dtos);
    }

    public async Task<Result> CancelRequestAsync(int sellerId, int requestId, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct);
        if (request is null)
            return Result.Failure("Request not found");

        var listing = await _listingRepo.GetByIdAsync(request.ListingId, ct);
        if (listing is null || listing.SellerId != sellerId)
            return Result.Failure("Access denied");

        if (request.RequestStatus != 1)
            return Result.Failure("Only pending requests can be cancelled");

        request.RequestStatus = 4; // Cancelled
        _requestRepo.Update(request);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ===== Inspector =====

    public async Task<Result<List<InspectionRequestDto>>> GetPendingRequestsAsync(CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindAsync(r => r.RequestStatus == 1, ct);

        var dtos = new List<InspectionRequestDto>();
        foreach (var req in requests.OrderBy(r => r.RequestDate))
            dtos.Add(await MapToRequestDtoAsync(req, ct));

        return Result<List<InspectionRequestDto>>.Success(dtos);
    }

    public async Task<Result<List<InspectionRequestDto>>> GetMyAssignedRequestsAsync(int inspectorId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindAsync(r => r.InspectorId == inspectorId, ct);

        var dtos = new List<InspectionRequestDto>();
        foreach (var req in requests.OrderByDescending(r => r.RequestDate))
            dtos.Add(await MapToRequestDtoAsync(req, ct));

        return Result<List<InspectionRequestDto>>.Success(dtos);
    }

    public async Task<Result> AcceptRequestAsync(int inspectorId, int requestId, CancellationToken ct = default)
    {
        var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);
        if (inspector is null || inspector.RoleId != 4)
            return Result.Failure("Only inspectors can accept requests");

        var request = await _requestRepo.GetByIdAsync(requestId, ct);
        if (request is null)
            return Result.Failure("Request not found");

        if (request.RequestStatus != 1)
            return Result.Failure("Only pending requests can be accepted");

        request.InspectorId = inspectorId;
        request.RequestStatus = 2; // InProgress
        _requestRepo.Update(request);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<InspectionReportDto>> UploadReportAsync(int inspectorId, UploadInspectionReportDto dto, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(dto.RequestId, ct);
        if (request is null)
            return Result<InspectionReportDto>.Failure("Request not found");

        if (request.InspectorId != inspectorId)
            return Result<InspectionReportDto>.Failure("You are not assigned to this request");

        if (request.RequestStatus != 2)
            return Result<InspectionReportDto>.Failure("Request must be InProgress to upload report");

        var existingReport = await _reportRepo.AnyAsync(r => r.RequestId == dto.RequestId, ct);
        if (existingReport)
            return Result<InspectionReportDto>.Failure("A report has already been uploaded for this request");

        var report = new InspectionReport
        {
            RequestId = dto.RequestId,
            FrameCheck = dto.FrameCheck,
            BrakeCheck = dto.BrakeCheck,
            TransmissionCheck = dto.TransmissionCheck,
            InspectorNote = dto.InspectorNote,
            FinalVerdict = dto.FinalVerdict,
            ReportUrl = dto.ReportUrl,
            CompletedAt = DateTime.UtcNow
        };

        await _reportRepo.AddAsync(report, ct);

        // Auto-complete the request
        request.RequestStatus = 3; // Completed
        _requestRepo.Update(request);

        await _uow.SaveChangesAsync(ct);

        var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);
        var listing = await _listingRepo.GetByIdAsync(request.ListingId, ct);

        return Result<InspectionReportDto>.Success(MapToReportDto(report, request, inspector, listing));
    }

    public async Task<Result<List<InspectionReportDto>>> GetMyReportsAsync(int inspectorId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindAsync(r => r.InspectorId == inspectorId, ct);
        var dtos = new List<InspectionReportDto>();

        foreach (var request in requests.OrderByDescending(r => r.RequestDate))
        {
            var reports = await _reportRepo.FindAsync(r => r.RequestId == request.RequestId, ct);
            var report = reports.FirstOrDefault();
            if (report is null) continue;

            var inspector = await _userRepo.GetByIdAsync(inspectorId, ct);
            var listing = await _listingRepo.GetByIdAsync(request.ListingId, ct);
            dtos.Add(MapToReportDto(report, request, inspector, listing));
        }

        return Result<List<InspectionReportDto>>.Success(dtos);
    }

    // ===== Public =====

    public async Task<Result<InspectionReportDto>> GetByListingAsync(int listingId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindAsync(r => r.ListingId == listingId && r.RequestStatus == 3, ct);
        var request = requests.OrderByDescending(r => r.RequestDate).FirstOrDefault();
        if (request is null)
            return Result<InspectionReportDto>.Failure("No completed inspection found for this listing");

        var reports = await _reportRepo.FindAsync(r => r.RequestId == request.RequestId, ct);
        var report = reports.FirstOrDefault();
        if (report is null)
            return Result<InspectionReportDto>.Failure("No inspection report found");

        var inspector = request.InspectorId.HasValue
            ? await _userRepo.GetByIdAsync(request.InspectorId.Value, ct)
            : null;
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);

        return Result<InspectionReportDto>.Success(MapToReportDto(report, request, inspector, listing));
    }

    public async Task<Result<InspectionReportDto>> GetByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct);
        if (request is null)
            return Result<InspectionReportDto>.Failure("Request not found");

        var reports = await _reportRepo.FindAsync(r => r.RequestId == requestId, ct);
        var report = reports.FirstOrDefault();
        if (report is null)
            return Result<InspectionReportDto>.Failure("No report found for this request");

        var inspector = request.InspectorId.HasValue
            ? await _userRepo.GetByIdAsync(request.InspectorId.Value, ct)
            : null;
        var listing = await _listingRepo.GetByIdAsync(request.ListingId, ct);

        return Result<InspectionReportDto>.Success(MapToReportDto(report, request, inspector, listing));
    }

    // ===== Mapping =====

    private async Task<InspectionRequestDto> MapToRequestDtoAsync(InspectionRequest request, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(request.ListingId, ct);
        var seller = listing is not null ? await _userRepo.GetByIdAsync(listing.SellerId, ct) : null;
        var inspector = request.InspectorId.HasValue
            ? await _userRepo.GetByIdAsync(request.InspectorId.Value, ct)
            : null;

        var hasReport = await _reportRepo.AnyAsync(r => r.RequestId == request.RequestId, ct);

        return new InspectionRequestDto
        {
            RequestId = request.RequestId,
            ListingId = request.ListingId,
            ListingTitle = listing?.Title ?? "Unknown",
            SellerId = listing?.SellerId ?? 0,
            SellerName = seller?.Username ?? "Unknown",
            InspectorId = request.InspectorId,
            InspectorName = inspector?.Username,
            RequestStatus = request.RequestStatus,
            RequestStatusLabel = GetStatusLabel(request.RequestStatus),
            RequestDate = request.RequestDate,
            HasReport = hasReport
        };
    }

    private static InspectionReportDto MapToReportDto(InspectionReport report, InspectionRequest request, User? inspector, BicycleListing? listing)
    {
        return new InspectionReportDto
        {
            ReportId = report.ReportId,
            RequestId = report.RequestId,
            ListingId = request.ListingId,
            ListingTitle = listing?.Title ?? "Unknown",
            RequestStatus = request.RequestStatus,
            RequestStatusLabel = GetStatusLabel(request.RequestStatus),
            FrameCheck = report.FrameCheck,
            BrakeCheck = report.BrakeCheck,
            TransmissionCheck = report.TransmissionCheck,
            InspectorNote = report.InspectorNote,
            FinalVerdict = report.FinalVerdict,
            FinalVerdictLabel = GetVerdictLabel(report.FinalVerdict),
            ReportUrl = report.ReportUrl,
            CompletedAt = report.CompletedAt,
            InspectorName = inspector?.Username ?? "Unknown",
            BikeTitle = listing?.Title ?? "Unknown"
        };
    }

    private static string GetStatusLabel(byte? status) => status switch
    {
        1 => "Pending",
        2 => "InProgress",
        3 => "Completed",
        4 => "Cancelled",
        _ => "Unknown"
    };

    private static string GetVerdictLabel(byte? verdict) => verdict switch
    {
        1 => "Pass",
        2 => "Fail",
        3 => "Conditional",
        _ => "Unknown"
    };
}
