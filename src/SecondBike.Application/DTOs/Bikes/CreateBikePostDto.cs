using Microsoft.AspNetCore.Http;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for creating a new bicycle listing.
/// Supports both file uploads (Images) and pre-uploaded URLs (ImageUrls).
/// </summary>
public class CreateBikePostDto
{
    // Listing info
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
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

    // Image file uploads (uploaded to Cloudinary)
    public List<IFormFile> Images { get; set; } = new();

    // Pre-uploaded image URLs (optional, for backward compatibility)
    public List<string> ImageUrls { get; set; } = new();
}
