using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Categories;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for managing bike categories (BikeType).
/// Provides CRUD operations for the admin.
/// </summary>
public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CategoryDto>> GetByIdAsync(int typeId, CancellationToken ct = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int typeId, CancellationToken ct = default);
}
