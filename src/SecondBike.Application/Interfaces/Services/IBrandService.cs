using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Brands;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for managing brands.
/// Provides CRUD operations for the admin.
/// </summary>
public interface IBrandService
{
    Task<Result<List<BrandDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<BrandDto>> GetByIdAsync(int brandId, CancellationToken ct = default);
    Task<Result<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken ct = default);
    Task<Result<BrandDto>> UpdateAsync(UpdateBrandDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int brandId, CancellationToken ct = default);
}
