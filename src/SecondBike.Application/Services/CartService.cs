using AutoMapper;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Buyer Experience — Shopping cart management.
/// Uses IBikeListingRepository for efficient eager-loaded queries.
/// </summary>
public class CartService : ICartService
{
    private readonly IRepository<ShoppingCart> _cartRepo;
    private readonly IBikeListingRepository _bikeListingRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CartService(
        IRepository<ShoppingCart> cartRepo,
        IBikeListingRepository bikeListingRepo,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _cartRepo = cartRepo;
        _bikeListingRepo = bikeListingRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result> AddAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var listing = await _bikeListingRepo.GetWithDetailsAsync(listingId, ct);
        if (listing is null)
            return Result.Failure("Listing not found");

        if (listing.ListingStatus != 1)
            return Result.Failure("Listing is not available");

        if (listing.SellerId == userId)
            return Result.Failure("You cannot add your own listing to cart");

        var exists = await _cartRepo.AnyAsync(c => c.UserId == userId && c.ListingId == listingId, ct);
        if (exists)
            return Result.Failure("Already in cart");

        await _cartRepo.AddAsync(new ShoppingCart
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
        var items = await _cartRepo.FindAsync(c => c.UserId == userId && c.ListingId == listingId, ct);
        var item = items.FirstOrDefault();
        if (item is null)
            return Result.Failure("Not in cart");

        _cartRepo.Delete(item);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<List<BikePostDto>>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        var cartItems = await _cartRepo.FindAsync(c => c.UserId == userId, ct);
        var listingIds = cartItems.Select(c => c.ListingId).ToList();

        var dtos = new List<BikePostDto>();
        foreach (var listingId in listingIds)
        {
            var listing = await _bikeListingRepo.GetWithDetailsAsync(listingId, ct);
            if (listing is null) continue;

            dtos.Add(_mapper.Map<BikePostDto>(listing));
        }

        return Result<List<BikePostDto>>.Success(dtos);
    }

    public async Task<Result> ClearAsync(int userId, CancellationToken ct = default)
    {
        var items = await _cartRepo.FindAsync(c => c.UserId == userId, ct);
        foreach (var item in items)
            _cartRepo.Delete(item);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<bool>> IsInCartAsync(int userId, int listingId, CancellationToken ct = default)
    {
        var exists = await _cartRepo.AnyAsync(c => c.UserId == userId && c.ListingId == listingId, ct);
        return Result<bool>.Success(exists);
    }

    public async Task<Result<int>> GetCountAsync(int userId, CancellationToken ct = default)
    {
        var count = await _cartRepo.CountAsync(c => c.UserId == userId, ct);
        return Result<int>.Success(count);
    }
}
