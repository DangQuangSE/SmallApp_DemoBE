namespace SecondBike.Application.DTOs.Categories;

public class CategoryDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int TotalBicycles { get; set; }
}

public class CreateCategoryDto
{
    public string TypeName { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
