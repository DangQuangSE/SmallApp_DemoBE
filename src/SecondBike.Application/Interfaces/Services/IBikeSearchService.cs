using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for bike search and browsing (Buyer Experience).
/// </summary>
public interface IBikeSearchService
{
    Task<Result<PagedResult<BikePostDto>>> SearchAsync(BikeFilterDto filter, CancellationToken ct = default);
    Task<Result<BikePostDto>> GetDetailAsync(int listingId, CancellationToken ct = default);
    Task<Result<List<string>>> GetBrandsAsync(CancellationToken ct = default);
}
