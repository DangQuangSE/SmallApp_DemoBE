using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for shopping cart management (Buyer Experience).
/// </summary>
public interface ICartService
{
    Task<Result> AddAsync(int userId, int listingId, CancellationToken ct = default);
    Task<Result> RemoveAsync(int userId, int listingId, CancellationToken ct = default);
    Task<Result<List<BikePostDto>>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<Result> ClearAsync(int userId, CancellationToken ct = default);
    Task<Result<bool>> IsInCartAsync(int userId, int listingId, CancellationToken ct = default);
    Task<Result<int>> GetCountAsync(int userId, CancellationToken ct = default);
}
