using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class BikeType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Bicycle> Bicycles { get; set; } = new List<Bicycle>();
}
