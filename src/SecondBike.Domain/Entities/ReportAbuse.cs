using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class ReportAbuse
{
    public int ReportAbuseId { get; set; }

    public int RequestAbuseId { get; set; }

    public int AdminId { get; set; }

    public string? Resolution { get; set; }

    public byte? Status { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual User Admin { get; set; } = null!;

    public virtual RequestAbuse RequestAbuse { get; set; } = null!;
}
