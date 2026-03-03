using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class InspectionReport
{
    public int ReportId { get; set; }

    public int RequestId { get; set; }

    public string? FrameCheck { get; set; }

    public string? BrakeCheck { get; set; }

    public string? TransmissionCheck { get; set; }

    public string? InspectorNote { get; set; }

    public byte? FinalVerdict { get; set; }

    public string? ReportUrl { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual InspectionRequest Request { get; set; } = null!;
}
