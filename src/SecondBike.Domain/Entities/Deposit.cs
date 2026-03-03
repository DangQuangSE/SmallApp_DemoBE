using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class Deposit
{
    public int DepositId { get; set; }

    public int OrderId { get; set; }

    public decimal? Amount { get; set; }

    public byte? Status { get; set; }

    public DateTime? DepositDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
