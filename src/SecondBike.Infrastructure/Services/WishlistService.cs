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
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<Bicycle> _bikeRepo;
    private readonly IRepository<Brand> _brandRepo;
    private readonly IRepository<BikeType> _typeRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<ListingMedium> _mediaRepo;
    private readonly IUnitOfWork _uow;

    public WishlistService(
        IRepository<Wishlist> wishlistRepo,
        IRepository<BicycleListing> listingRepo,
        IRepository<Bicycle> bikeRepo,
        IRepository<Brand> brandRepo,
        IRepository<BikeType> typeRepo,
        IRepository<User> userRepo,
        IRepository<ListingMedium> mediaRepo,
        IUnitOfWork uow)
    {
        _wishlistRepo = wishlistRepo;
        _listingRepo = listingRepo;
        _bikeRepo = bikeRepo;
        _brandRepo = brandRepo;
        _typeRepo = typeRepo;
        _userRepo = userRepo;
        _mediaRepo = mediaRepo;
        _uow = uow;
    }

    public async Task<Result> AddAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var exists = await _wishlistRepo.AnyAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        if (exists) return Result.Failure("Already in wishlist");

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
            var listing = await _listingRepo.GetByIdAsync(w.ListingId, ct);
            if (listing is null) continue;

            var bike = await _bikeRepo.GetByIdAsync(listing.BikeId, ct);
            var seller = await _userRepo.GetByIdAsync(listing.SellerId, ct);
            var media = await _mediaRepo.FindAsync(m => m.ListingId == listing.ListingId, ct);

            Brand? brand = null;
            BikeType? bikeType = null;
            if (bike?.BrandId.HasValue == true)
                brand = await _brandRepo.GetByIdAsync(bike.BrandId.Value, ct);
            if (bike?.TypeId.HasValue == true)
                bikeType = await _typeRepo.GetByIdAsync(bike.TypeId.Value, ct);

            dtos.Add(new BikePostDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                ListingStatus = listing.ListingStatus,
                Address = listing.Address,
                PostedDate = listing.PostedDate,
                BikeId = listing.BikeId,
                ModelName = bike?.ModelName,
                Color = bike?.Color,
                Condition = bike?.Condition,
                BrandName = brand?.BrandName,
                TypeName = bikeType?.TypeName,
                SellerId = listing.SellerId,
                SellerName = seller?.Username ?? "Unknown",
                Images = media.Select(m => new BikeImageDto
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    IsThumbnail = m.IsThumbnail
                }).ToList()
            });
        }

        return Result<List<BikePostDto>>.Success(dtos);
    }

    public async Task<Result<bool>> IsInWishlistAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var exists = await _wishlistRepo.AnyAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        return Result<bool>.Success(exists);
    }
}
