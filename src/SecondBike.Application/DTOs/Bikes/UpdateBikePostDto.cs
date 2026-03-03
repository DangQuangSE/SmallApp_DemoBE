namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for updating an existing bicycle listing.
/// </summary>
public class UpdateBikePostDto
{
    public int ListingId { get; set; }

    // Listing info
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Address { get; set; }

    // Bicycle info
    public int? BrandId { get; set; }
    public int? TypeId { get; set; }
    public string? ModelName { get; set; }
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    public string? Condition { get; set; }

    // Bicycle detail
    public string? FrameSize { get; set; }
    public string? FrameMaterial { get; set; }
    public string? WheelSize { get; set; }
    public string? BrakeType { get; set; }
    public decimal? Weight { get; set; }
    public string? Transmission { get; set; }

    // Images
    public List<string> ImageUrls { get; set; } = new();
}
