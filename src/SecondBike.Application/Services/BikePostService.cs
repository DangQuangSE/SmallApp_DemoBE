using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Seller Core — Manages bicycle listing CRUD operations.
/// Business logic belongs in Application layer.
/// Uses IBikeListingRepository for complex EF queries (Include).
/// </summary>
public class BikePostService : IBikePostService
{
    private readonly IBikeListingRepository _bikeListingRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<Bicycle> _bikeRepo;
    private readonly IRepository<BicycleDetail> _detailRepo;
    private readonly IRepository<ListingMedium> _mediaRepo;
    private readonly IRepository<Wishlist> _wishlistRepo;
    private readonly IRepository<ShoppingCart> _cartRepo;
    private readonly IRepository<OrderDetail> _orderDetailRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IImageStorageService _imageStorage;
    private readonly IMapper _mapper;
    private readonly ILogger<BikePostService> _logger;

    private const string ImageFolder = "listings";

    public BikePostService(
        IBikeListingRepository bikeListingRepo,
        IRepository<BicycleListing> listingRepo,
        IRepository<Bicycle> bikeRepo,
        IRepository<BicycleDetail> detailRepo,
        IRepository<ListingMedium> mediaRepo,
        IRepository<Wishlist> wishlistRepo,
        IRepository<ShoppingCart> cartRepo,
        IRepository<OrderDetail> orderDetailRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow,
        IImageStorageService imageStorage,
        IMapper mapper,
        ILogger<BikePostService> logger)
    {
        _bikeListingRepo = bikeListingRepo;
        _listingRepo = listingRepo;
        _bikeRepo = bikeRepo;
        _detailRepo = detailRepo;
        _mediaRepo = mediaRepo;
        _wishlistRepo = wishlistRepo;
        _cartRepo = cartRepo;
        _orderDetailRepo = orderDetailRepo;
        _userRepo = userRepo;
        _uow = uow;
        _imageStorage = imageStorage;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BikePostDto>> CreateAsync(int sellerId, CreateBikePostDto dto, CancellationToken ct = default)
    {
        var seller = await _userRepo.GetByIdAsync(sellerId, ct);
        if (seller is null) return Result<BikePostDto>.Failure("Seller not found");

        var uploadedUrls = await UploadImagesAsync(dto.Images);
        var allImageUrls = uploadedUrls.Concat(dto.ImageUrls).ToList();
        if (allImageUrls.Count == 0)
            return Result<BikePostDto>.Failure("At least one image is required");

        var bike = _mapper.Map<Bicycle>(dto);
        await _bikeRepo.AddAsync(bike, ct);
        await _uow.SaveChangesAsync(ct);

        var detail = _mapper.Map<BicycleDetail>(dto);
        detail.BikeId = bike.BikeId;
        await _detailRepo.AddAsync(detail, ct);

        var listing = _mapper.Map<BicycleListing>(dto);
        listing.SellerId = sellerId;
        listing.BikeId = bike.BikeId;
        listing.ListingStatus = 2;
        listing.PostedDate = DateTime.UtcNow;
        await _listingRepo.AddAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await CreateMediaRecordsAsync(listing.ListingId, allImageUrls, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Listing {ListingId} created by seller {SellerId} with {ImageCount} images",
            listing.ListingId, sellerId, allImageUrls.Count);

        return Result<BikePostDto>.Success(await BuildDtoAsync(listing.ListingId, ct));
    }

    public async Task<Result<BikePostDto>> UpdateAsync(int sellerId, UpdateBikePostDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null) return Result<BikePostDto>.Failure("Listing not found");
        if (listing.SellerId != sellerId) return Result<BikePostDto>.Failure("You can only edit your own listings");

        _mapper.Map(dto, listing);
        _listingRepo.Update(listing);

        var bike = await _bikeRepo.GetByIdAsync(listing.BikeId, ct);
        if (bike is not null)
        {
            _mapper.Map(dto, bike);
            _bikeRepo.Update(bike);

            var details = await _detailRepo.FindAsync(d => d.BikeId == bike.BikeId, ct);
            var detail = details.FirstOrDefault();
            if (detail is not null)
            {
                _mapper.Map(dto, detail);
                _detailRepo.Update(detail);
            }
        }

        if (dto.RemoveMediaIds.Count > 0)
        {
            await RemoveMediaAsync(listing.ListingId, dto.RemoveMediaIds, ct);
        }

        var uploadedUrls = await UploadImagesAsync(dto.NewImages);
        var allNewUrls = uploadedUrls.Concat(dto.ImageUrls).ToList();
        if (allNewUrls.Count > 0)
        {
            await CreateMediaRecordsAsync(listing.ListingId, allNewUrls, ct);
        }

        if (dto.ThumbnailMediaId.HasValue)
        {
            await SetThumbnailAsync(listing.ListingId, dto.ThumbnailMediaId.Value, ct);
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Listing {ListingId} updated by seller {SellerId}", listing.ListingId, sellerId);

        return Result<BikePostDto>.Success(await BuildDtoAsync(listing.ListingId, ct));
    }

    public async Task<Result> DeleteAsync(int sellerId, int listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);
        if (listing is null) return Result.Failure("Listing not found");
        if (listing.SellerId != sellerId) return Result.Failure("You can only delete your own listings");

        // Cascade delete related records (matching DB trigger TR_BicycleListing_CascadeDelete)
        var wishlists = await _wishlistRepo.FindAsync(w => w.ListingId == listingId, ct);
        foreach (var w in wishlists)
            _wishlistRepo.Delete(w);

        var carts = await _cartRepo.FindAsync(c => c.ListingId == listingId, ct);
        foreach (var c in carts)
            _cartRepo.Delete(c);

        var media = await _mediaRepo.FindAsync(m => m.ListingId == listingId, ct);
        foreach (var m in media)
        {
            await SafeDeleteImageAsync(m.MediaUrl);
            _mediaRepo.Delete(m);
        }

        var orderDetails = await _orderDetailRepo.FindAsync(od => od.ListingId == listingId, ct);
        foreach (var od in orderDetails)
            _orderDetailRepo.Delete(od);

        var bikeId = listing.BikeId;
        _listingRepo.Delete(listing);

        var details = await _detailRepo.FindAsync(d => d.BikeId == bikeId, ct);
        foreach (var detail in details)
            _detailRepo.Delete(detail);

        var otherListings = await _listingRepo.AnyAsync(l => l.BikeId == bikeId && l.ListingId != listingId, ct);
        if (!otherListings)
        {
            var bike = await _bikeRepo.GetByIdAsync(bikeId, ct);
            if (bike is not null)
                _bikeRepo.Delete(bike);
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Listing {ListingId} cascade-deleted by seller {SellerId}", listingId, sellerId);

        return Result.Success();
    }

    public async Task<Result> ToggleVisibilityAsync(int sellerId, int listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);
        if (listing is null) return Result.Failure("Listing not found");
        if (listing.SellerId != sellerId) return Result.Failure("You can only modify your own listings");

        listing.ListingStatus = listing.ListingStatus == 1 ? (byte)0 : (byte)1;
        _listingRepo.Update(listing);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<BikePostDto>> GetByIdAsync(int listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);
        if (listing is null) return Result<BikePostDto>.Failure("Listing not found");
        return Result<BikePostDto>.Success(await BuildDtoAsync(listingId, ct));
    }

    public async Task<Result<List<BikePostDto>>> GetBySellerAsync(int sellerId, CancellationToken ct = default)
    {
        var listings = await _bikeListingRepo.GetBySellerWithDetailsAsync(sellerId, ct);
        return Result<List<BikePostDto>>.Success(_mapper.Map<List<BikePostDto>>(listings));
    }

    #region Private Helpers

    private async Task<List<string>> UploadImagesAsync(List<IFormFile> images)
    {
        var urls = new List<string>();
        foreach (var image in images)
        {
            await using var stream = image.OpenReadStream();
            var url = await _imageStorage.UploadAsync(stream, image.FileName, ImageFolder);
            urls.Add(url);
        }
        return urls;
    }

    private async Task CreateMediaRecordsAsync(int listingId, List<string> imageUrls, CancellationToken ct)
    {
        var existingMedia = await _mediaRepo.FindAsync(m => m.ListingId == listingId, ct);
        var hasThumbnail = existingMedia.Any(m => m.IsThumbnail == true);

        for (int i = 0; i < imageUrls.Count; i++)
        {
            await _mediaRepo.AddAsync(new ListingMedium
            {
                ListingId = listingId,
                MediaUrl = imageUrls[i],
                MediaType = "image",
                IsThumbnail = !hasThumbnail && i == 0
            }, ct);
        }
    }

    private async Task RemoveMediaAsync(int listingId, List<int> mediaIds, CancellationToken ct)
    {
        var mediaToRemove = await _mediaRepo.FindAsync(
            m => m.ListingId == listingId && mediaIds.Contains(m.MediaId), ct);

        foreach (var media in mediaToRemove)
        {
            await SafeDeleteImageAsync(media.MediaUrl);
            _mediaRepo.Delete(media);
        }
    }

    private async Task SetThumbnailAsync(int listingId, int mediaId, CancellationToken ct)
    {
        var allMedia = await _mediaRepo.FindAsync(m => m.ListingId == listingId, ct);
        foreach (var media in allMedia)
        {
            media.IsThumbnail = media.MediaId == mediaId;
            _mediaRepo.Update(media);
        }
    }

    private async Task SafeDeleteImageAsync(string imageUrl)
    {
        try
        {
            await _imageStorage.DeleteAsync(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image from cloud storage: {ImageUrl}", imageUrl);
        }
    }

    private async Task<BikePostDto> BuildDtoAsync(int listingId, CancellationToken ct)
    {
        var listing = await _bikeListingRepo.GetWithDetailsAsync(listingId, ct)
            ?? throw new InvalidOperationException($"Listing {listingId} not found");

        return _mapper.Map<BikePostDto>(listing);
    }

    #endregion
}

