using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Seller Core — Manages bicycle listing CRUD operations.
/// </summary>
public class BikePostService : IBikePostService
{
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<Bicycle> _bikeRepo;
    private readonly IRepository<BicycleDetail> _detailRepo;
    private readonly IRepository<ListingMedium> _mediaRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Brand> _brandRepo;
    private readonly IRepository<BikeType> _typeRepo;
    private readonly IUnitOfWork _uow;

    public BikePostService(
        IRepository<BicycleListing> listingRepo,
        IRepository<Bicycle> bikeRepo,
        IRepository<BicycleDetail> detailRepo,
        IRepository<ListingMedium> mediaRepo,
        IRepository<User> userRepo,
        IRepository<Brand> brandRepo,
        IRepository<BikeType> typeRepo,
        IUnitOfWork uow)
    {
        _listingRepo = listingRepo;
        _bikeRepo = bikeRepo;
        _detailRepo = detailRepo;
        _mediaRepo = mediaRepo;
        _userRepo = userRepo;
        _brandRepo = brandRepo;
        _typeRepo = typeRepo;
        _uow = uow;
    }

    public async Task<Result<BikePostDto>> CreateAsync(int sellerId, CreateBikePostDto dto, CancellationToken ct = default)
    {
        var seller = await _userRepo.GetByIdAsync(sellerId, ct);
        if (seller is null) return Result<BikePostDto>.Failure("Seller not found");

        // Create bicycle
        var bike = new Bicycle
        {
            BrandId = dto.BrandId,
            TypeId = dto.TypeId,
            ModelName = dto.ModelName,
            SerialNumber = dto.SerialNumber,
            Color = dto.Color,
            Condition = dto.Condition
        };
        await _bikeRepo.AddAsync(bike, ct);
        await _uow.SaveChangesAsync(ct);

        // Create bicycle detail
        var detail = new BicycleDetail
        {
            BikeId = bike.BikeId,
            FrameSize = dto.FrameSize,
            FrameMaterial = dto.FrameMaterial,
            WheelSize = dto.WheelSize,
            BrakeType = dto.BrakeType,
            Weight = dto.Weight,
            Transmission = dto.Transmission
        };
        await _detailRepo.AddAsync(detail, ct);

        // Create listing
        var listing = new BicycleListing
        {
            SellerId = sellerId,
            BikeId = bike.BikeId,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Address = dto.Address,
            ListingStatus = 1, // Active
            PostedDate = DateTime.UtcNow
        };
        await _listingRepo.AddAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        // Create media
        for (int i = 0; i < dto.ImageUrls.Count; i++)
        {
            await _mediaRepo.AddAsync(new ListingMedium
            {
                ListingId = listing.ListingId,
                MediaUrl = dto.ImageUrls[i],
                MediaType = "image",
                IsThumbnail = i == 0
            }, ct);
        }
        await _uow.SaveChangesAsync(ct);

        return Result<BikePostDto>.Success(await BuildDtoAsync(listing, ct));
    }

    public async Task<Result<BikePostDto>> UpdateAsync(int sellerId, UpdateBikePostDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null) return Result<BikePostDto>.Failure("Listing not found");
        if (listing.SellerId != sellerId) return Result<BikePostDto>.Failure("You can only edit your own listings");

        listing.Title = dto.Title;
        listing.Description = dto.Description;
        listing.Price = dto.Price;
        listing.Address = dto.Address;
        _listingRepo.Update(listing);

        // Update bicycle
        var bike = await _bikeRepo.GetByIdAsync(listing.BikeId, ct);
        if (bike is not null)
        {
            bike.BrandId = dto.BrandId;
            bike.TypeId = dto.TypeId;
            bike.ModelName = dto.ModelName;
            bike.SerialNumber = dto.SerialNumber;
            bike.Color = dto.Color;
            bike.Condition = dto.Condition;
            _bikeRepo.Update(bike);

            // Update detail
            var details = await _detailRepo.FindAsync(d => d.BikeId == bike.BikeId, ct);
            var detail = details.FirstOrDefault();
            if (detail is not null)
            {
                detail.FrameSize = dto.FrameSize;
                detail.FrameMaterial = dto.FrameMaterial;
                detail.WheelSize = dto.WheelSize;
                detail.BrakeType = dto.BrakeType;
                detail.Weight = dto.Weight;
                detail.Transmission = dto.Transmission;
                _detailRepo.Update(detail);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result<BikePostDto>.Success(await BuildDtoAsync(listing, ct));
    }

    public async Task<Result> DeleteAsync(int sellerId, int listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);
        if (listing is null) return Result.Failure("Listing not found");
        if (listing.SellerId != sellerId) return Result.Failure("You can only delete your own listings");

        _listingRepo.Delete(listing);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ToggleVisibilityAsync(int sellerId, int listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);
        if (listing is null) return Result.Failure("Listing not found");
        if (listing.SellerId != sellerId) return Result.Failure("You can only modify your own listings");

        // Toggle between 1 (Active) and 0 (Hidden)
        listing.ListingStatus = listing.ListingStatus == 1 ? (byte)0 : (byte)1;
        _listingRepo.Update(listing);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<BikePostDto>> GetByIdAsync(int listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId, ct);
        if (listing is null) return Result<BikePostDto>.Failure("Listing not found");
        return Result<BikePostDto>.Success(await BuildDtoAsync(listing, ct));
    }

    public async Task<Result<List<BikePostDto>>> GetBySellerAsync(int sellerId, CancellationToken ct = default)
    {
        var listings = await _listingRepo.FindAsync(l => l.SellerId == sellerId, ct);
        var dtos = new List<BikePostDto>();
        foreach (var listing in listings)
        {
            dtos.Add(await BuildDtoAsync(listing, ct));
        }
        return Result<List<BikePostDto>>.Success(dtos);
    }

    private async Task<BikePostDto> BuildDtoAsync(BicycleListing listing, CancellationToken ct)
    {
        var bike = await _bikeRepo.GetByIdAsync(listing.BikeId, ct);
        var seller = await _userRepo.GetByIdAsync(listing.SellerId, ct);
        var media = await _mediaRepo.FindAsync(m => m.ListingId == listing.ListingId, ct);

        Brand? brand = null;
        BikeType? bikeType = null;
        BicycleDetail? detail = null;
        bool hasInspection = false;

        if (bike is not null)
        {
            if (bike.BrandId.HasValue)
                brand = await _brandRepo.GetByIdAsync(bike.BrandId.Value, ct);
            if (bike.TypeId.HasValue)
                bikeType = await _typeRepo.GetByIdAsync(bike.TypeId.Value, ct);

            var details = await _detailRepo.FindAsync(d => d.BikeId == bike.BikeId, ct);
            detail = details.FirstOrDefault();
        }

        return new BikePostDto
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
            SerialNumber = bike?.SerialNumber,
            Color = bike?.Color,
            Condition = bike?.Condition,
            BrandName = brand?.BrandName,
            TypeName = bikeType?.TypeName,
            FrameSize = detail?.FrameSize,
            FrameMaterial = detail?.FrameMaterial,
            WheelSize = detail?.WheelSize,
            BrakeType = detail?.BrakeType,
            Weight = detail?.Weight,
            Transmission = detail?.Transmission,
            SellerId = listing.SellerId,
            SellerName = seller?.Username ?? "Unknown",
            HasInspection = hasInspection,
            Images = media.Select(m => new BikeImageDto
            {
                MediaId = m.MediaId,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                IsThumbnail = m.IsThumbnail
            }).ToList()
        };
    }
}
