using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO returned when viewing a bike post.
/// </summary>
public class BikePostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PostStatus Status { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public BikeCategory Category { get; set; }
    public BikeSize Size { get; set; }
    public string FrameMaterial { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public BikeCondition Condition { get; set; }
    public decimal WeightKg { get; set; }
    public int? OdometerKm { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int WishlistCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Seller info
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerAvatar { get; set; }
    public decimal SellerRating { get; set; }
    public bool IsVerifiedSeller { get; set; }

    // Images
    public List<BikeImageDto> Images { get; set; } = new();

    // Inspection
    public bool HasInspection { get; set; }
    public string? InspectionSummary { get; set; }
}

public class BikeImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
