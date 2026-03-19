using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for Admin to manage the central catalogue of bicycles.
/// </summary>
public interface IBicycleAdminService
{
    Task<Result<int>> CreateBicycleAsync(CreateBicycleDto dto, CancellationToken ct = default);
    Task<Result<List<BicycleDto>>> GetAllBicyclesAsync(CancellationToken ct = default);
}
