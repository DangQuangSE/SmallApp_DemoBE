using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for updating an existing bike post.
/// </summary>
public class UpdateBikePostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
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
    public string? UsageHistory { get; set; }
    public bool HasAccidents { get; set; }
    public string? AccidentDescription { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
}
