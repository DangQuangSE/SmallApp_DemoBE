using System.ComponentModel.DataAnnotations;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// DTO for Admin to create a new generic bicycle model in the catalog.
/// Contains fixed hardware specifications of the bike.
/// </summary>
public class CreateBicycleDto
{
    // Bicycle Core
    [Required(ErrorMessage = "Brand is required")]
    public int? BrandId { get; set; }
    
    [Required(ErrorMessage = "Type is required")]
    public int? TypeId { get; set; }
    
    [Required(ErrorMessage = "Model Name is required")]
    public string? ModelName { get; set; }
    
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    
    // Bicycle Details
    public string? FrameSize { get; set; }
    public string? FrameMaterial { get; set; }
    public string? WheelSize { get; set; }
    public string? BrakeType { get; set; }
    public decimal? Weight { get; set; }
    public string? Transmission { get; set; }
}
