using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Abuse;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Abuse report management.
/// Buyer submits RequestAbuse ? Admin resolves by creating ReportAbuse
/// with optional actions (ban user, hide listing).
/// </summary>
public class AbuseService : IAbuseService
{
    private readonly IRepository<RequestAbuse> _requestRepo;
    private readonly IRepository<ReportAbuse> _reportRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IUnitOfWork _uow;

    public AbuseService(
        IRepository<RequestAbuse> requestRepo,
        IRepository<ReportAbuse> reportRepo,
        IRepository<User> userRepo,
        IRepository<BicycleListing> listingRepo,
        IUnitOfWork uow)
    {
        _requestRepo = requestRepo;
        _reportRepo = reportRepo;
        _userRepo = userRepo;
        _listingRepo = listingRepo;
        _uow = uow;
    }

    public async Task<Result<AbuseRequestDto>> SubmitAsync(int reporterId, CreateAbuseRequestDto dto, CancellationToken ct = default)
    {
        if (dto.TargetListingId is null && dto.TargetUserId is null)
            return Result<AbuseRequestDto>.Failure("Must specify either a listing or a user to report");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            return Result<AbuseRequestDto>.Failure("Reason is required");

        if (dto.TargetUserId == reporterId)
            return Result<AbuseRequestDto>.Failure("You cannot report yourself");

        if (dto.TargetListingId.HasValue)
        {
            var listing = await _listingRepo.GetByIdAsync(dto.TargetListingId.Value, ct);
            if (listing is null)
                return Result<AbuseRequestDto>.Failure("Target listing not found");

            if (listing.SellerId == reporterId)
                return Result<AbuseRequestDto>.Failure("You cannot report your own listing");
        }

        if (dto.TargetUserId.HasValue)
        {
            var target = await _userRepo.GetByIdAsync(dto.TargetUserId.Value, ct);
            if (target is null)
                return Result<AbuseRequestDto>.Failure("Target user not found");
        }

        var duplicate = await _requestRepo.AnyAsync(r =>
            r.ReporterId == reporterId
            && r.TargetListingId == dto.TargetListingId
            && r.TargetUserId == dto.TargetUserId
            && r.ReportAbuse == null, ct);
        if (duplicate)
            return Result<AbuseRequestDto>.Failure("You have already submitted a pending report for this target");

        var request = new RequestAbuse
        {
            ReporterId = reporterId,
            TargetListingId = dto.TargetListingId,
            TargetUserId = dto.TargetUserId,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow
        };

        await _requestRepo.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<AbuseRequestDto>.Success(await MapRequestAsync(request, ct));
    }

    public async Task<Result<List<AbuseRequestDto>>> GetMyRequestsAsync(int userId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindWithIncludesAsync(
            r => r.ReporterId == userId, ct, r => r.ReportAbuse!);

        var dtos = new List<AbuseRequestDto>();
        foreach (var r in requests.OrderByDescending(r => r.CreatedAt))
            dtos.Add(await MapRequestAsync(r, ct));

        return Result<List<AbuseRequestDto>>.Success(dtos);
    }

    public async Task<Result<List<AbuseRequestDto>>> GetPendingRequestsAsync(CancellationToken ct = default)
    {
        var requests = await _requestRepo.FindWithIncludesAsync(
            r => r.ReportAbuse == null, ct, r => r.ReportAbuse!);

        var dtos = new List<AbuseRequestDto>();
        foreach (var r in requests.OrderBy(r => r.CreatedAt))
            dtos.Add(await MapRequestAsync(r, ct));

        return Result<List<AbuseRequestDto>>.Success(dtos);
    }

    public async Task<Result<List<AbuseReportDto>>> GetAllReportsAsync(CancellationToken ct = default)
    {
        var reports = await _reportRepo.FindWithIncludesAsync(
            _ => true, ct, r => r.RequestAbuse);

        var dtos = new List<AbuseReportDto>();
        foreach (var report in reports.OrderByDescending(r => r.ResolvedAt))
        {
            var admin = await _userRepo.GetByIdAsync(report.AdminId, ct);
            var requestDto = await MapRequestAsync(report.RequestAbuse, ct);

            dtos.Add(new AbuseReportDto
            {
                ReportAbuseId = report.ReportAbuseId,
                RequestAbuseId = report.RequestAbuseId,
                AdminName = admin?.Username ?? "Unknown",
                Resolution = report.Resolution,
                Status = report.Status,
                ResolvedAt = report.ResolvedAt,
                Request = requestDto
            });
        }

        return Result<List<AbuseReportDto>>.Success(dtos);
    }

    public async Task<Result<AbuseReportDto>> ResolveAsync(int adminId, ResolveAbuseRequestDto dto, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(dto.RequestAbuseId, ct);
        if (request is null)
            return Result<AbuseReportDto>.Failure("Abuse request not found");

        var existingReport = await _reportRepo.AnyAsync(r => r.RequestAbuseId == dto.RequestAbuseId, ct);
        if (existingReport)
            return Result<AbuseReportDto>.Failure("This request has already been resolved");

        if (dto.BanTargetUser && request.TargetUserId.HasValue)
        {
            var target = await _userRepo.GetByIdAsync(request.TargetUserId.Value, ct);
            if (target is not null)
            {
                target.Status = 0; // Banned
                _userRepo.Update(target);
            }
        }

        if (dto.HideTargetListing && request.TargetListingId.HasValue)
        {
            var listing = await _listingRepo.GetByIdAsync(request.TargetListingId.Value, ct);
            if (listing is not null)
            {
                listing.ListingStatus = 0; // Hidden
                _listingRepo.Update(listing);
            }
        }

        var report = new ReportAbuse
        {
            RequestAbuseId = dto.RequestAbuseId,
            AdminId = adminId,
            Resolution = dto.Resolution,
            Status = dto.Status,
            ResolvedAt = DateTime.UtcNow
        };

        await _reportRepo.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);

        var admin = await _userRepo.GetByIdAsync(adminId, ct);
        var requestDto = await MapRequestAsync(request, ct);

        return Result<AbuseReportDto>.Success(new AbuseReportDto
        {
            ReportAbuseId = report.ReportAbuseId,
            RequestAbuseId = report.RequestAbuseId,
            AdminName = admin?.Username ?? "Unknown",
            Resolution = report.Resolution,
            Status = report.Status,
            ResolvedAt = report.ResolvedAt,
            Request = requestDto
        });
    }

    private async Task<AbuseRequestDto> MapRequestAsync(RequestAbuse request, CancellationToken ct)
    {
        var reporter = await _userRepo.GetByIdAsync(request.ReporterId, ct);

        string? targetListingTitle = null;
        if (request.TargetListingId.HasValue)
        {
            var listing = await _listingRepo.GetByIdAsync(request.TargetListingId.Value, ct);
            targetListingTitle = listing?.Title;
        }

        string? targetUserName = null;
        if (request.TargetUserId.HasValue)
        {
            var target = await _userRepo.GetByIdAsync(request.TargetUserId.Value, ct);
            targetUserName = target?.Username;
        }

        return new AbuseRequestDto
        {
            RequestAbuseId = request.RequestAbuseId,
            ReporterId = request.ReporterId,
            ReporterName = reporter?.Username ?? "Unknown",
            TargetListingId = request.TargetListingId,
            TargetListingTitle = targetListingTitle,
            TargetUserId = request.TargetUserId,
            TargetUserName = targetUserName,
            Reason = request.Reason ?? string.Empty,
            CreatedAt = request.CreatedAt,
            IsResolved = request.ReportAbuse is not null
        };
    }
}
