using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// Filter criteria for bike search.
/// </summary>
public class BikeFilterDto
{
    public string? SearchTerm { get; set; }
    public BikeCategory? Category { get; set; }
    public BikeSize? Size { get; set; }
    public BikeCondition? Condition { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public string? SortBy { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
