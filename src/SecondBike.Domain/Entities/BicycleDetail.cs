using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class BicycleDetail
{
    public int DetailId { get; set; }

    public int BikeId { get; set; }

    public string? FrameSize { get; set; }

    public string? FrameMaterial { get; set; }

    public string? WheelSize { get; set; }

    public string? BrakeType { get; set; }

    public decimal? Weight { get; set; }

    public string? Transmission { get; set; }

    public virtual Bicycle Bike { get; set; } = null!;
}
