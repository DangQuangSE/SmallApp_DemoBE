using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class Brand
{
    public int BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public string? Country { get; set; }

    public virtual ICollection<Bicycle> Bicycles { get; set; } = new List<Bicycle>();
}
