namespace SecondBike.Application.DTOs.Inspections;

// ===== Seller — Yêu c?u ki?m ??nh =====

public class CreateInspectionRequestDto
{
    public int ListingId { get; set; }
    public string? Note { get; set; }
}

public class InspectionRequestDto
{
    public int RequestId { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int? InspectorId { get; set; }
    public string? InspectorName { get; set; }
    public byte? RequestStatus { get; set; }
    public string RequestStatusLabel { get; set; } = string.Empty;
    public DateTime? RequestDate { get; set; }
    public bool HasReport { get; set; }
}

// ===== Inspector — Upload report =====

public class CreateInspectionDto
{
    public int ListingId { get; set; }
    public string? FrameCheck { get; set; }
    public string? BrakeCheck { get; set; }
    public string? TransmissionCheck { get; set; }
    public string? InspectorNote { get; set; }
    public byte? FinalVerdict { get; set; }
    public string? ReportUrl { get; set; }
}

public class UploadInspectionReportDto
{
    public int RequestId { get; set; }
    public string? FrameCheck { get; set; }
    public string? BrakeCheck { get; set; }
    public string? TransmissionCheck { get; set; }
    public string? InspectorNote { get; set; }
    public byte? FinalVerdict { get; set; }
    public string? ReportUrl { get; set; }
}

public class InspectionReportDto
{
    public int ReportId { get; set; }
    public int RequestId { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public byte? RequestStatus { get; set; }
    public string RequestStatusLabel { get; set; } = string.Empty;
    public string? FrameCheck { get; set; }
    public string? BrakeCheck { get; set; }
    public string? TransmissionCheck { get; set; }
    public string? InspectorNote { get; set; }
    public byte? FinalVerdict { get; set; }
    public string FinalVerdictLabel { get; set; } = string.Empty;
    public string? ReportUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string BikeTitle { get; set; } = string.Empty;
}
