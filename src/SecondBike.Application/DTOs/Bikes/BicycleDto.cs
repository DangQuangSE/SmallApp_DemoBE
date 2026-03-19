namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for retrieving Bicycle models with full details.
/// Designed for Admin Dashboard grids and Frontend Dropdowns.
/// </summary>
public class BicycleDto
{
    public int BikeId { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? ModelName { get; set; }
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    
    // Details from BicycleDetail entity
    public string? FrameSize { get; set; }
    public string? FrameMaterial { get; set; }
    public string? WheelSize { get; set; }
    public string? BrakeType { get; set; }
    public decimal? Weight { get; set; }
    public string? Transmission { get; set; }
}
