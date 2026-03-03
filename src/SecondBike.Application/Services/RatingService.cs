using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Ratings;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Interaction — Seller rating/feedback service.
/// Business logic belongs in Application layer.
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRepository<Feedback> _feedbackRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderDetail> _orderDetailRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IUnitOfWork _uow;

    public RatingService(
        IRepository<Feedback> feedbackRepo,
        IRepository<Order> orderRepo,
        IRepository<OrderDetail> orderDetailRepo,
        IRepository<User> userRepo,
        IRepository<BicycleListing> listingRepo,
        IUnitOfWork uow)
    {
        _feedbackRepo = feedbackRepo;
        _orderRepo = orderRepo;
        _orderDetailRepo = orderDetailRepo;
        _userRepo = userRepo;
        _listingRepo = listingRepo;
        _uow = uow;
    }

    public async Task<Result<RatingDto>> CreateAsync(int fromUserId, CreateRatingDto dto, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result<RatingDto>.Failure("Order not found");
        if (order.BuyerId != fromUserId) return Result<RatingDto>.Failure("Only the buyer can rate");
        if (order.OrderStatus != 4) return Result<RatingDto>.Failure("Order must be completed first");

        var existing = await _feedbackRepo.AnyAsync(f => f.OrderId == dto.OrderId && f.UserId == fromUserId, ct);
        if (existing) return Result<RatingDto>.Failure("This order has already been rated");

        var details = await _orderDetailRepo.FindAsync(d => d.OrderId == order.OrderId, ct);
        var firstDetail = details.FirstOrDefault();
        if (firstDetail is null) return Result<RatingDto>.Failure("Order has no items");

        var listing = await _listingRepo.GetByIdAsync(firstDetail.ListingId, ct);
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
            OrderId = feedback.OrderId,
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
                OrderId = f.OrderId,
                Rating = f.Rating,
                Comment = f.Comment,
                FromUserName = fromUser?.Username ?? "Unknown",
                CreatedAt = f.CreatedAt
            });
        }

        return Result<List<RatingDto>>.Success(dtos);
    }

    public async Task<Result<SellerStatsDto>> GetSellerStatsAsync(int sellerId, CancellationToken ct = default)
    {
        var seller = await _userRepo.GetByIdAsync(sellerId, ct);
        if (seller is null) return Result<SellerStatsDto>.Failure("Seller not found");

        var feedbacks = await _feedbackRepo.FindAsync(f => f.TargetUserId == sellerId, ct);
        var rated = feedbacks.Where(f => f.Rating.HasValue).ToList();

        var distribution = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
        };
        foreach (var f in rated)
        {
            distribution[f.Rating!.Value]++;
        }

        return Result<SellerStatsDto>.Success(new SellerStatsDto
        {
            SellerId = sellerId,
            SellerName = seller.Username,
            AverageRating = rated.Count > 0 ? Math.Round(rated.Average(f => f.Rating!.Value), 1) : 0,
            TotalReviews = feedbacks.Count,
            RatingDistribution = distribution
        });
    }

    public async Task<Result<bool>> HasRatedOrderAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var exists = await _feedbackRepo.AnyAsync(f => f.OrderId == orderId && f.UserId == userId, ct);
        return Result<bool>.Success(exists);
    }
}
