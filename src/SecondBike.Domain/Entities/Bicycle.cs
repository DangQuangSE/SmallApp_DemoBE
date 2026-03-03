using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class Bicycle
{
    public int BikeId { get; set; }

    public int? BrandId { get; set; }

    public int? TypeId { get; set; }

    public string? ModelName { get; set; }

    public string? SerialNumber { get; set; }

    public string? Color { get; set; }

    public string? Condition { get; set; }

    public virtual BicycleDetail? BicycleDetail { get; set; }

    public virtual ICollection<BicycleListing> BicycleListings { get; set; } = new List<BicycleListing>();

    public virtual Brand? Brand { get; set; }

    public virtual BikeType? Type { get; set; }
}
