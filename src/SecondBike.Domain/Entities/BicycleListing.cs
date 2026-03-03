using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class BicycleListing
{
    public int ListingId { get; set; }

    public int SellerId { get; set; }

    public int BikeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public byte? ListingStatus { get; set; }

    public string? Address { get; set; }

    public DateTime? PostedDate { get; set; }

    public virtual Bicycle Bike { get; set; } = null!;

    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    public virtual ICollection<InspectionRequest> InspectionRequests { get; set; } = new List<InspectionRequest>();

    public virtual ICollection<ListingMedium> ListingMedia { get; set; } = new List<ListingMedium>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<RequestAbuse> RequestAbuses { get; set; } = new List<RequestAbuse>();

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
