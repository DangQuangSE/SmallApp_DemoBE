using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for managing bike posts (Seller Core — Team Member 1).
/// </summary>
public interface IBikePostService
{
    Task<Result<BikePostDto>> CreateAsync(Guid sellerId, CreateBikePostDto dto, CancellationToken ct = default);
    Task<Result<BikePostDto>> UpdateAsync(Guid sellerId, UpdateBikePostDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid sellerId, Guid postId, CancellationToken ct = default);
    Task<Result> ToggleVisibilityAsync(Guid sellerId, Guid postId, CancellationToken ct = default);
    Task<Result> SubmitForModerationAsync(Guid sellerId, Guid postId, CancellationToken ct = default);
    Task<Result<BikePostDto>> GetByIdAsync(Guid postId, CancellationToken ct = default);
    Task<Result<List<BikePostDto>>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default);
}
