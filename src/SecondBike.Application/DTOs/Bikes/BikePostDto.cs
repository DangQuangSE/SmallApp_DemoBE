namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO returned when viewing a bicycle listing.
/// Maps to BicycleListing + Bicycle + Brand + BikeType + ListingMedia + User.
/// </summary>
public class BikePostDto
{
    public int ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public byte? ListingStatus { get; set; }
    public string? Address { get; set; }
    public DateTime? PostedDate { get; set; }

    // Bicycle info
    public int BikeId { get; set; }
    public string? ModelName { get; set; }
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    public string? Condition { get; set; }
    public string? BrandName { get; set; }
    public string? TypeName { get; set; }

    // Bicycle detail
    public string? FrameSize { get; set; }
    public string? FrameMaterial { get; set; }
    public string? WheelSize { get; set; }
    public string? BrakeType { get; set; }
    public decimal? Weight { get; set; }
    public string? Transmission { get; set; }

    // Seller info
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;

    // Images
    public List<BikeImageDto> Images { get; set; } = new();

    // Inspection
    public bool HasInspection { get; set; }
}

public class BikeImageDto
{
    public int MediaId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string? MediaType { get; set; }
    public bool? IsThumbnail { get; set; }
}
