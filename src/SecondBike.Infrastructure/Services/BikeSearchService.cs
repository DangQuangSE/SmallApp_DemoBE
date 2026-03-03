using Microsoft.EntityFrameworkCore;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Buyer Experience — Search and filter bicycle listings.
/// </summary>
public class BikeSearchService : IBikeSearchService
{
    private readonly SecondBikeDbContext _context;

    public BikeSearchService(SecondBikeDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<BikePostDto>>> SearchAsync(BikeFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.BicycleListings
            .Include(l => l.Bike).ThenInclude(b => b.Brand)
            .Include(l => l.Bike).ThenInclude(b => b.Type)
            .Include(l => l.Bike).ThenInclude(b => b.BicycleDetail)
            .Include(l => l.Seller)
            .Include(l => l.ListingMedia)
            .Where(l => l.ListingStatus == 1) // Active
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(term) ||
                (l.Description != null && l.Description.ToLower().Contains(term)) ||
                (l.Bike.ModelName != null && l.Bike.ModelName.ToLower().Contains(term)));
        }

        if (filter.BrandId.HasValue)
            query = query.Where(l => l.Bike.BrandId == filter.BrandId.Value);

        if (filter.TypeId.HasValue)
            query = query.Where(l => l.Bike.TypeId == filter.TypeId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Condition))
            query = query.Where(l => l.Bike.Condition == filter.Condition);

        if (filter.MinPrice.HasValue)
            query = query.Where(l => l.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(l => l.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Address))
            query = query.Where(l => l.Address != null && l.Address.ToLower().Contains(filter.Address.ToLower()));

        query = filter.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(l => l.Price),
            "price_desc" => query.OrderByDescending(l => l.Price),
            "oldest" => query.OrderBy(l => l.PostedDate),
            _ => query.OrderByDescending(l => l.PostedDate)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();

        return Result<PagedResult<BikePostDto>>.Success(new PagedResult<BikePostDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<BikePostDto>> GetDetailAsync(int listingId, CancellationToken ct = default)
    {
        var listing = await _context.BicycleListings
            .Include(l => l.Bike).ThenInclude(b => b.Brand)
            .Include(l => l.Bike).ThenInclude(b => b.Type)
            .Include(l => l.Bike).ThenInclude(b => b.BicycleDetail)
            .Include(l => l.Seller)
            .Include(l => l.ListingMedia)
            .FirstOrDefaultAsync(l => l.ListingId == listingId, ct);

        if (listing is null)
            return Result<BikePostDto>.Failure("Listing not found");

        return Result<BikePostDto>.Success(MapToDto(listing));
    }

    public async Task<Result<List<string>>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = await _context.Brands
            .Select(b => b.BrandName)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(ct);

        return Result<List<string>>.Success(brands);
    }

    private static BikePostDto MapToDto(BicycleListing listing)
    {
        var bike = listing.Bike;
        var detail = bike?.BicycleDetail;

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
            BrandName = bike?.Brand?.BrandName,
            TypeName = bike?.Type?.TypeName,
            FrameSize = detail?.FrameSize,
            FrameMaterial = detail?.FrameMaterial,
            WheelSize = detail?.WheelSize,
            BrakeType = detail?.BrakeType,
            Weight = detail?.Weight,
            Transmission = detail?.Transmission,
            SellerId = listing.SellerId,
            SellerName = listing.Seller?.Username ?? "Unknown",
            Images = listing.ListingMedia.Select(m => new BikeImageDto
            {
                MediaId = m.MediaId,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                IsThumbnail = m.IsThumbnail
            }).ToList()
        };
    }
}
