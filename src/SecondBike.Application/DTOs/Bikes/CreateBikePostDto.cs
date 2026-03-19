using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for creating a new bicycle listing.
/// Supports both file uploads (Images) and pre-uploaded URLs (ImageUrls).
/// Requires an existing BikeId selected from the catalog.
/// </summary>
public class CreateBikePostDto
{
    // Reference to existing Bicycle
    [Required]
    public int BikeId { get; set; }

    // Listing info
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Address { get; set; }

    // Image file uploads (uploaded to Cloudinary)
    public List<IFormFile> Images { get; set; } = new();

    // Pre-uploaded image URLs (optional, for backward compatibility)
    public List<string> ImageUrls { get; set; } = new();
}
