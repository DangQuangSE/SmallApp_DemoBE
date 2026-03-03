using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Ratings;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for seller ratings/feedback (Interaction).
/// </summary>
public interface IRatingService
{
    Task<Result<RatingDto>> CreateAsync(int fromUserId, CreateRatingDto dto, CancellationToken ct = default);
    Task<Result<List<RatingDto>>> GetBySellerAsync(int sellerId, CancellationToken ct = default);
}
