using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Ratings;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for seller ratings (Interaction — Team Member 3).
/// </summary>
public interface IRatingService
{
    Task<Result<RatingDto>> CreateAsync(Guid fromUserId, CreateRatingDto dto, CancellationToken ct = default);
    Task<Result<List<RatingDto>>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default);
    Task<Result> RespondAsync(Guid sellerId, Guid ratingId, string response, CancellationToken ct = default);
}
