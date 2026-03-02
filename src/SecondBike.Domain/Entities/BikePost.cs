using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// A bicycle listing posted by a seller.
/// </summary>
public class BikePost : BaseEntity
{
    public Guid SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;

    // Specifications
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public BikeCategory Category { get; set; }
    public BikeSize Size { get; set; }
    public string FrameMaterial { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public BikeCondition Condition { get; set; }
    public decimal WeightKg { get; set; }

    // Usage
    public int? OdometerKm { get; set; }
    public string? UsageHistory { get; set; }
    public bool HasAccidents { get; set; }
    public string? AccidentDescription { get; set; }

    // Location
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;

    // Moderation
    public DateTime? PublishedAt { get; set; }
    public Guid? ModeratedBy { get; set; }
    public string? ModerationNotes { get; set; }
    public string? RejectionReason { get; set; }

    // Metrics
    public int ViewCount { get; set; }
    public int WishlistCount { get; set; }

    // Navigation
    public virtual AppUser Seller { get; set; } = null!;
    public virtual ICollection<BikeImage> Images { get; set; } = new List<BikeImage>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual InspectionReport? InspectionReport { get; set; }
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
