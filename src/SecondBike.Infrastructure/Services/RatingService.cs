using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Ratings;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Interaction — Seller rating/feedback service using the Feedback entity.
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRepository<Feedback> _feedbackRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IUnitOfWork _uow;

    public RatingService(
        IRepository<Feedback> feedbackRepo,
        IRepository<Order> orderRepo,
        IRepository<User> userRepo,
        IRepository<BicycleListing> listingRepo,
        IUnitOfWork uow)
    {
        _feedbackRepo = feedbackRepo;
        _orderRepo = orderRepo;
        _userRepo = userRepo;
        _listingRepo = listingRepo;
        _uow = uow;
    }

    public async Task<Result<RatingDto>> CreateAsync(int fromUserId, CreateRatingDto dto, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result<RatingDto>.Failure("Order not found");
        if (order.BuyerId != fromUserId) return Result<RatingDto>.Failure("Only the buyer can rate");
        if (order.OrderStatus != 4) return Result<RatingDto>.Failure("Order must be completed first"); // 4 = Completed

        var existing = await _feedbackRepo.AnyAsync(f => f.OrderId == dto.OrderId && f.UserId == fromUserId, ct);
        if (existing) return Result<RatingDto>.Failure("This order has already been rated");

        // Find seller via listing
        var listing = await _listingRepo.GetByIdAsync(order.ListingId, ct);
        if (listing is null) return Result<RatingDto>.Failure("Listing not found");

        var feedback = new Feedback
        {
            UserId = fromUserId,
            TargetUserId = listing.SellerId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackRepo.AddAsync(feedback, ct);
        await _uow.SaveChangesAsync(ct);

        var fromUser = await _userRepo.GetByIdAsync(fromUserId, ct);
        return Result<RatingDto>.Success(new RatingDto
        {
            FeedbackId = feedback.FeedbackId,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            FromUserName = fromUser?.Username ?? "Unknown",
            CreatedAt = feedback.CreatedAt
        });
    }

    public async Task<Result<List<RatingDto>>> GetBySellerAsync(int sellerId, CancellationToken ct = default)
    {
        var feedbacks = await _feedbackRepo.FindAsync(f => f.TargetUserId == sellerId, ct);
        var dtos = new List<RatingDto>();

        foreach (var f in feedbacks.OrderByDescending(f => f.CreatedAt))
        {
            var fromUser = await _userRepo.GetByIdAsync(f.UserId, ct);
            dtos.Add(new RatingDto
            {
                FeedbackId = f.FeedbackId,
                Rating = f.Rating,
                Comment = f.Comment,
                FromUserName = fromUser?.Username ?? "Unknown",
                CreatedAt = f.CreatedAt
            });
        }

        return Result<List<RatingDto>>.Success(dtos);
    }
}
