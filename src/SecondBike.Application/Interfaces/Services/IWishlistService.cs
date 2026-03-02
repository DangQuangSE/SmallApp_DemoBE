using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for user wishlists (Buyer Experience — Team Member 4).
/// </summary>
public interface IWishlistService
{
    Task<Result> AddAsync(Guid userId, Guid bikePostId, CancellationToken ct = default);
    Task<Result> RemoveAsync(Guid userId, Guid bikePostId, CancellationToken ct = default);
    Task<Result<List<BikePostDto>>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<bool>> IsInWishlistAsync(Guid userId, Guid bikePostId, CancellationToken ct = default);
}
