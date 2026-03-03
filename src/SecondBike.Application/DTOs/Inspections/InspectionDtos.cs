namespace SecondBike.Application.DTOs.Inspections;

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

public class InspectionReportDto
{
    public int ReportId { get; set; }
    public int RequestId { get; set; }
    public byte? RequestStatus { get; set; }
    public string? FrameCheck { get; set; }
    public string? BrakeCheck { get; set; }
    public string? TransmissionCheck { get; set; }
    public string? InspectorNote { get; set; }
    public byte? FinalVerdict { get; set; }
    public string? ReportUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string BikeTitle { get; set; } = string.Empty;
}
