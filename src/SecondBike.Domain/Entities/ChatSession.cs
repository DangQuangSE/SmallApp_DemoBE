using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class ChatSession
{
    public int SessionId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public int? ListingId { get; set; }

    public DateTime? StartedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual BicycleListing? Listing { get; set; }

    public virtual User Seller { get; set; } = null!;
}
