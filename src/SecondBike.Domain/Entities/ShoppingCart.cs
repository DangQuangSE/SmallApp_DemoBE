using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class ShoppingCart
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public int ListingId { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual BicycleListing Listing { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
