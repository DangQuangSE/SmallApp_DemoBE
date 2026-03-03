using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for user wishlists (Buyer Experience).
/// </summary>
public interface IWishlistService
{
    Task<Result> AddAsync(int userId, int listingId, CancellationToken ct = default);
    Task<Result> RemoveAsync(int userId, int listingId, CancellationToken ct = default);
    Task<Result<List<BikePostDto>>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<Result<bool>> IsInWishlistAsync(int userId, int listingId, CancellationToken ct = default);
}
