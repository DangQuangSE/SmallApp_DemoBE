using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class AuditLogSystem
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? Ipaddress { get; set; }

    public string? Details { get; set; }

    public DateTime? LogDate { get; set; }

    public virtual User? User { get; set; }
}
