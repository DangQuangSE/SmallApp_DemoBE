using Microsoft.AspNetCore.Http;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for updating an existing bicycle listing.
/// Supports adding new images, removing existing ones, and setting thumbnail.
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

    // New image file uploads (uploaded to Cloudinary)
    public List<IFormFile> NewImages { get; set; } = new();

    // Pre-uploaded image URLs (optional, for backward compatibility)
    public List<string> ImageUrls { get; set; } = new();

    // IDs of existing images to remove
    public List<int> RemoveMediaIds { get; set; } = new();

    // ID of existing image to set as thumbnail
    public int? ThumbnailMediaId { get; set; }
}
