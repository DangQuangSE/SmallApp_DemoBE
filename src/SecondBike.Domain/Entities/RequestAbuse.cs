using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class RequestAbuse
{
    public int RequestAbuseId { get; set; }

    public int ReporterId { get; set; }

    public int? TargetListingId { get; set; }

    public int? TargetUserId { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ReportAbuse? ReportAbuse { get; set; }

    public virtual User Reporter { get; set; } = null!;

    public virtual BicycleListing? TargetListing { get; set; }

    public virtual User? TargetUser { get; set; }
}
