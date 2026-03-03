using AutoMapper;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Buyer Experience — Wishlist management.
/// Uses IBikeListingRepository for efficient eager-loaded queries.
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly IRepository<Wishlist> _wishlistRepo;
    private readonly IBikeListingRepository _bikeListingRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public WishlistService(
        IRepository<Wishlist> wishlistRepo,
        IBikeListingRepository bikeListingRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _wishlistRepo = wishlistRepo;
        _bikeListingRepo = bikeListingRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result> AddAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var listing = await _bikeListingRepo.GetWithDetailsAsync(listingId, ct);
        if (listing is null)
            return Result.Failure("Listing not found");

        if (listing.SellerId == userId)
            return Result.Failure("You cannot add your own listing to wishlist");

        var exists = await _wishlistRepo.AnyAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        if (exists)
            return Result.Failure("Already in wishlist");

        await _wishlistRepo.AddAsync(new Wishlist
        {
            UserId = userId,
            ListingId = listingId,
            AddedDate = DateTime.UtcNow
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var items = await _wishlistRepo.FindAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        var item = items.FirstOrDefault();
        if (item is null) return Result.Failure("Not in wishlist");

        _wishlistRepo.Delete(item);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<List<BikePostDto>>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        var wishlists = await _wishlistRepo.FindAsync(w => w.UserId == userId, ct);
        var dtos = new List<BikePostDto>();

        foreach (var w in wishlists)
        {
            var listing = await _bikeListingRepo.GetWithDetailsAsync(w.ListingId, ct);
            if (listing is null) continue;

            dtos.Add(_mapper.Map<BikePostDto>(listing));
        }

        return Result<List<BikePostDto>>.Success(dtos);
    }

    public async Task<Result<bool>> IsInWishlistAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var exists = await _wishlistRepo.AnyAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        return Result<bool>.Success(exists);
    }
}
