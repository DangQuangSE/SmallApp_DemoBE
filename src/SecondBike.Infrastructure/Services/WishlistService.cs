using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Buyer Experience — Wishlist management.
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly IRepository<Wishlist> _wishlistRepo;
    private readonly IRepository<BikePost> _postRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly IUnitOfWork _uow;

    public WishlistService(
        IRepository<Wishlist> wishlistRepo,
        IRepository<BikePost> postRepo,
        IRepository<AppUser> userRepo,
        IUnitOfWork uow)
    {
        _wishlistRepo = wishlistRepo;
        _postRepo = postRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result> AddAsync(Guid userId, Guid bikePostId, CancellationToken ct = default)
    {
        var exists = await _wishlistRepo.AnyAsync(w => w.UserId == userId && w.BikePostId == bikePostId, ct);
        if (exists) return Result.Failure("Already in wishlist");

        await _wishlistRepo.AddAsync(new Wishlist { UserId = userId, BikePostId = bikePostId }, ct);

        var post = await _postRepo.GetByIdAsync(bikePostId, ct);
        if (post is not null)
        {
            post.WishlistCount++;
            _postRepo.Update(post);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(Guid userId, Guid bikePostId, CancellationToken ct = default)
    {
        var items = await _wishlistRepo.FindAsync(w => w.UserId == userId && w.BikePostId == bikePostId, ct);
        var item = items.FirstOrDefault();
        if (item is null) return Result.Failure("Not in wishlist");

        _wishlistRepo.Delete(item);

        var post = await _postRepo.GetByIdAsync(bikePostId, ct);
        if (post is not null && post.WishlistCount > 0)
        {
            post.WishlistCount--;
            _postRepo.Update(post);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<List<BikePostDto>>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var wishlists = await _wishlistRepo.FindAsync(w => w.UserId == userId, ct);
        var dtos = new List<BikePostDto>();

        foreach (var w in wishlists)
        {
            var post = await _postRepo.GetByIdAsync(w.BikePostId, ct);
            if (post is null) continue;

            var seller = await _userRepo.GetByIdAsync(post.SellerId, ct);
            dtos.Add(new BikePostDto
            {
                Id = post.Id,
                Title = post.Title,
                Price = post.Price,
                Brand = post.Brand,
                Model = post.Model,
                Category = post.Category,
                Condition = post.Condition,
                City = post.City,
                District = post.District,
                Status = post.Status,
                SellerId = seller?.Id ?? Guid.Empty,
                SellerName = seller?.FullName ?? "Unknown",
                SellerRating = seller?.SellerRating ?? 0,
                Images = post.Images.Select(i => new BikeImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).OrderBy(i => i.DisplayOrder).ToList()
            });
        }

        return Result<List<BikePostDto>>.Success(dtos);
    }

    public async Task<Result<bool>> IsInWishlistAsync(Guid userId, Guid bikePostId, CancellationToken ct = default)
    {
        var exists = await _wishlistRepo.AnyAsync(w => w.UserId == userId && w.BikePostId == bikePostId, ct);
        return Result<bool>.Success(exists);
    }
}
