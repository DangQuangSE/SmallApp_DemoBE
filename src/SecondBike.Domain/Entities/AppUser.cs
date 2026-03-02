using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Application user with profile, role, and seller information.
/// </summary>
public class AppUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Buyer;
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Seller-specific
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public bool IsVerifiedSeller { get; set; }
    public decimal SellerRating { get; set; }
    public int TotalRatingsCount { get; set; }

    // Auth link — maps to ASP.NET Identity UserId
    public string IdentityUserId { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<BikePost> BikePosts { get; set; } = new List<BikePost>();
    public virtual ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public virtual ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();
    public virtual ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
    public virtual ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
    public virtual ICollection<Message> MessagesSent { get; set; } = new List<Message>();
    public virtual ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
    public virtual ICollection<InspectionReport> InspectionReports { get; set; } = new List<InspectionReport>();
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public virtual Wallet? Wallet { get; set; }
}
