namespace SecondBike.Application.DTOs.Abuse;

public class CreateAbuseRequestDto
{
    public int? TargetListingId { get; set; }
    public int? TargetUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AbuseRequestDto
{
    public int RequestAbuseId { get; set; }
    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public int? TargetListingId { get; set; }
    public string? TargetListingTitle { get; set; }
    public int? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}

public class ResolveAbuseRequestDto
{
    public int RequestAbuseId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public byte Status { get; set; }
    public bool BanTargetUser { get; set; }
    public bool HideTargetListing { get; set; }
}

public class AbuseReportDto
{
    public int ReportAbuseId { get; set; }
    public int RequestAbuseId { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public byte? Status { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public AbuseRequestDto Request { get; set; } = null!;
}
