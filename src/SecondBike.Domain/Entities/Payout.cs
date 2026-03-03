using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class Payout
{
    public int PayoutId { get; set; }

    public int OrderId { get; set; }

    public decimal? AmountToSeller { get; set; }

    public byte? Status { get; set; }

    public DateTime? PayoutDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
