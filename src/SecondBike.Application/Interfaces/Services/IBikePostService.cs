using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for managing bicycle listings (Seller Core).
/// </summary>
public interface IBikePostService
{
    Task<Result<BikePostDto>> CreateAsync(int sellerId, CreateBikePostDto dto, CancellationToken ct = default);
    Task<Result<BikePostDto>> UpdateAsync(int sellerId, UpdateBikePostDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int sellerId, int listingId, CancellationToken ct = default);
    Task<Result> ToggleVisibilityAsync(int sellerId, int listingId, CancellationToken ct = default);
    Task<Result<BikePostDto>> GetByIdAsync(int listingId, CancellationToken ct = default);
    Task<Result<List<BikePostDto>>> GetBySellerAsync(int sellerId, CancellationToken ct = default);
}
