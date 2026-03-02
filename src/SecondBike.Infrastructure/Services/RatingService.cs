using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Ratings;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Interaction — Seller rating service.
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRepository<Rating> _ratingRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly IUnitOfWork _uow;

    public RatingService(
        IRepository<Rating> ratingRepo,
        IRepository<Order> orderRepo,
        IRepository<AppUser> userRepo,
        IUnitOfWork uow)
    {
        _ratingRepo = ratingRepo;
        _orderRepo = orderRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<RatingDto>> CreateAsync(Guid fromUserId, CreateRatingDto dto, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result<RatingDto>.Failure("Order not found");
        if (order.BuyerId != fromUserId) return Result<RatingDto>.Failure("Only the buyer can rate");
        if (order.Status != OrderStatus.Completed) return Result<RatingDto>.Failure("Order must be completed first");

        var existing = await _ratingRepo.AnyAsync(r => r.OrderId == dto.OrderId, ct);
        if (existing) return Result<RatingDto>.Failure("This order has already been rated");

        var rating = new Rating
        {
            OrderId = dto.OrderId,
            FromUserId = fromUserId,
            ToUserId = order.SellerId,
            Stars = dto.Stars,
            Comment = dto.Comment,
            CommunicationRating = dto.CommunicationRating,
            AccuracyRating = dto.AccuracyRating,
            PackagingRating = dto.PackagingRating,
            SpeedRating = dto.SpeedRating
        };

        await _ratingRepo.AddAsync(rating, ct);

        // Update seller's average rating
        var seller = await _userRepo.GetByIdAsync(order.SellerId, ct);
        if (seller is not null)
        {
            var allRatings = await _ratingRepo.FindAsync(r => r.ToUserId == seller.Id, ct);
            var totalStars = allRatings.Sum(r => r.Stars) + dto.Stars;
            var totalCount = allRatings.Count + 1;
            seller.SellerRating = Math.Round((decimal)totalStars / totalCount, 2);
            seller.TotalRatingsCount = totalCount;
            _userRepo.Update(seller);
        }

        await _uow.SaveChangesAsync(ct);

        var fromUser = await _userRepo.GetByIdAsync(fromUserId, ct);
        return Result<RatingDto>.Success(new RatingDto
        {
            Id = rating.Id,
            Stars = rating.Stars,
            Comment = rating.Comment,
            FromUserName = fromUser?.FullName ?? "Unknown",
            FromUserAvatar = fromUser?.AvatarUrl,
            CommunicationRating = rating.CommunicationRating,
            AccuracyRating = rating.AccuracyRating,
            PackagingRating = rating.PackagingRating,
            SpeedRating = rating.SpeedRating,
            CreatedAt = rating.CreatedAt
        });
    }

    public async Task<Result<List<RatingDto>>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default)
    {
        var ratings = await _ratingRepo.FindAsync(r => r.ToUserId == sellerId && r.IsPublic, ct);
        var dtos = new List<RatingDto>();

        foreach (var r in ratings.OrderByDescending(r => r.CreatedAt))
        {
            var fromUser = await _userRepo.GetByIdAsync(r.FromUserId, ct);
            dtos.Add(new RatingDto
            {
                Id = r.Id,
                Stars = r.Stars,
                Comment = r.Comment,
                FromUserName = fromUser?.FullName ?? "Unknown",
                FromUserAvatar = fromUser?.AvatarUrl,
                CommunicationRating = r.CommunicationRating,
                AccuracyRating = r.AccuracyRating,
                PackagingRating = r.PackagingRating,
                SpeedRating = r.SpeedRating,
                SellerResponse = r.SellerResponse,
                CreatedAt = r.CreatedAt
            });
        }

        return Result<List<RatingDto>>.Success(dtos);
    }

    public async Task<Result> RespondAsync(Guid sellerId, Guid ratingId, string response, CancellationToken ct = default)
    {
        var rating = await _ratingRepo.GetByIdAsync(ratingId, ct);
        if (rating is null) return Result.Failure("Rating not found");
        if (rating.ToUserId != sellerId) return Result.Failure("You can only respond to your own ratings");

        rating.SellerResponse = response;
        rating.SellerRespondedAt = DateTime.UtcNow;
        _ratingRepo.Update(rating);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
