namespace SecondBike.Application.DTOs.Brands;

public class BrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public int TotalBicycles { get; set; }
}

public class CreateBrandDto
{
    public string BrandName { get; set; } = string.Empty;
    public string? Country { get; set; }
}

public class UpdateBrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Country { get; set; }
}
