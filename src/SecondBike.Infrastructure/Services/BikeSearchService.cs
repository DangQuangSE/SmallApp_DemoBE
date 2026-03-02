using Microsoft.EntityFrameworkCore;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;
using SecondBike.Infrastructure.Data;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Buyer Experience � Search and filter bikes.
/// </summary>
public class BikeSearchService : IBikeSearchService
{
    private readonly AppDbContext _context;
    private readonly IRepository<AppUser> _userRepo;

    public BikeSearchService(AppDbContext context, IRepository<AppUser> userRepo)
    {
        _context = context;
        _userRepo = userRepo;
    }

    public async Task<Result<PagedResult<BikePostDto>>> SearchAsync(BikeFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.BikePosts
            .Include(p => p.Images)
            .Where(p => p.Status == PostStatus.Active && !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Brand.ToLower().Contains(term) ||
                p.Model.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        if (filter.Category.HasValue)
            query = query.Where(p => p.Category == filter.Category.Value);

        if (filter.Size.HasValue)
            query = query.Where(p => p.Size == filter.Size.Value);

        if (filter.Condition.HasValue)
            query = query.Where(p => p.Condition == filter.Condition.Value);

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand.ToLower() == filter.Brand.ToLower());

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(p => p.City.ToLower() == filter.City.ToLower());

        if (filter.MinYear.HasValue)
            query = query.Where(p => p.Year >= filter.MinYear.Value);

        if (filter.MaxYear.HasValue)
            query = query.Where(p => p.Year <= filter.MaxYear.Value);

        // Sort
        query = filter.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "oldest" => query.OrderBy(p => p.CreatedAt),
            "popular" => query.OrderByDescending(p => p.ViewCount),
            _ => query.OrderByDescending(p => p.CreatedAt) // newest
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);

        var sellerIds = items.Select(p => p.SellerId).Distinct().ToList();
        var users = await _userRepo.FindAsync(u => sellerIds.Contains(u.Id), ct);
        var sellers = users.ToDictionary(u => u.Id);

        var dtos = items.Select(p => MapToDto(p, sellers.GetValueOrDefault(p.SellerId))).ToList();

        return Result<PagedResult<BikePostDto>>.Success(new PagedResult<BikePostDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<BikePostDto>> GetDetailAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await _context.BikePosts
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, ct);
        if (post is null) return Result<BikePostDto>.Failure("Post not found");

        post.ViewCount++;
        await _context.SaveChangesAsync(ct);

        var seller = await _userRepo.GetByIdAsync(post.SellerId, ct);
        return Result<BikePostDto>.Success(MapToDto(post, seller));
    }

    public async Task<Result<List<string>>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = await _context.BikePosts
            .Where(p => p.Status == PostStatus.Active && !p.IsDeleted)
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(ct);
        return Result<List<string>>.Success(brands);
    }

    public async Task<Result<List<string>>> GetCitiesAsync(CancellationToken ct = default)
    {
        var cities = await _context.BikePosts
            .Where(p => p.Status == PostStatus.Active && !p.IsDeleted)
            .Select(p => p.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
        return Result<List<string>>.Success(cities);
    }

    private static BikePostDto MapToDto(BikePost post, AppUser? seller)
    {
        return new BikePostDto
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            Price = post.Price,
            Status = post.Status,
            Brand = post.Brand,
            Model = post.Model,
            Year = post.Year,
            Category = post.Category,
            Size = post.Size,
            FrameMaterial = post.FrameMaterial,
            Color = post.Color,
            Condition = post.Condition,
            WeightKg = post.WeightKg,
            OdometerKm = post.OdometerKm,
            City = post.City,
            District = post.District,
            ViewCount = post.ViewCount,
            WishlistCount = post.WishlistCount,
            CreatedAt = post.CreatedAt,
            PublishedAt = post.PublishedAt,
            SellerId = seller?.Id ?? Guid.Empty,
            SellerName = seller?.FullName ?? "Unknown",
            SellerAvatar = seller?.AvatarUrl,
            SellerRating = seller?.SellerRating ?? 0,
            IsVerifiedSeller = seller?.IsVerifiedSeller ?? false,
            Images = post.Images.Select(i => new BikeImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                DisplayOrder = i.DisplayOrder,
                IsPrimary = i.IsPrimary
            }).OrderBy(i => i.DisplayOrder).ToList()
        };
    }
}
