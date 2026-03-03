using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class ServiceFee
{
    public int FeeId { get; set; }

    public int OrderId { get; set; }

    public decimal? FeeAmount { get; set; }

    public string? Description { get; set; }

    public virtual Order Order { get; set; } = null!;
}
