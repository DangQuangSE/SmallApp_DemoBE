namespace SecondBike.Application.DTOs.Bikes;

/// <summary>
/// Filter criteria for bike search.
/// </summary>
public class BikeFilterDto
{
    public string? SearchTerm { get; set; }
    public int? BrandId { get; set; }
    public int? TypeId { get; set; }
    public string? Condition { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Address { get; set; }
    public string? SortBy { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
