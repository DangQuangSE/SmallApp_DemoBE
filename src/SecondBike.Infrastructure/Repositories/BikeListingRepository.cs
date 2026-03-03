using Microsoft.EntityFrameworkCore;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IBikeListingRepository.
/// Encapsulates complex Include/ThenInclude queries that require DbContext knowledge.
/// </summary>
public class BikeListingRepository : IBikeListingRepository
{
    private readonly SecondBikeDbContext _context;

    public BikeListingRepository(SecondBikeDbContext context)
    {
        _context = context;
    }

    public async Task<BicycleListing?> GetWithDetailsAsync(int listingId, CancellationToken ct = default)
    {
        return await FullQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.ListingId == listingId, ct);
    }

    public async Task<List<BicycleListing>> GetBySellerWithDetailsAsync(int sellerId, CancellationToken ct = default)
    {
        return await FullQuery()
            .Where(l => l.SellerId == sellerId)
            .ToListAsync(ct);
    }

    public async Task<(List<BicycleListing> Items, int TotalCount)> SearchAsync(BikeFilterDto filter, CancellationToken ct = default)
    {
        var query = FullQuery()
            .Where(l => l.ListingStatus == 1) // Active only
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

        if (!string.IsNullOrWhiteSpace(filter.FrameSize))
            query = query.Where(l => l.Bike.BicycleDetail != null && l.Bike.BicycleDetail.FrameSize == filter.FrameSize);

        if (!string.IsNullOrWhiteSpace(filter.WheelSize))
            query = query.Where(l => l.Bike.BicycleDetail != null && l.Bike.BicycleDetail.WheelSize == filter.WheelSize);

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

        return (items, totalCount);
    }

    public async Task<bool> HasOrderDetailsAsync(int listingId, CancellationToken ct = default)
    {
        return await _context.OrderDetails.AnyAsync(od => od.ListingId == listingId, ct);
    }

    private IQueryable<BicycleListing> FullQuery()
    {
        return _context.BicycleListings
            .Include(l => l.Bike).ThenInclude(b => b.Brand)
            .Include(l => l.Bike).ThenInclude(b => b.Type)
            .Include(l => l.Bike).ThenInclude(b => b.BicycleDetail)
            .Include(l => l.Seller)
            .Include(l => l.ListingMedia)
            .Include(l => l.InspectionRequests);
    }
}
