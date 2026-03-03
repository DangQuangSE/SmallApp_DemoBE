using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class InspectionRequest
{
    public int RequestId { get; set; }

    public int ListingId { get; set; }

    public int? InspectorId { get; set; }

    public byte? RequestStatus { get; set; }

    public DateTime? RequestDate { get; set; }

    public virtual InspectionReport? InspectionReport { get; set; }

    public virtual User? Inspector { get; set; }

    public virtual BicycleListing Listing { get; set; } = null!;
}
