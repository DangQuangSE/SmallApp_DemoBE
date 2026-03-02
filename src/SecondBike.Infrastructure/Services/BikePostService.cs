using Microsoft.EntityFrameworkCore;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Seller Core � Manages bike post CRUD operations.
/// </summary>
public class BikePostService : IBikePostService
{
    private readonly IRepository<BikePost> _postRepo;
    private readonly IRepository<BikeImage> _imageRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly IUnitOfWork _uow;

    public BikePostService(
        IRepository<BikePost> postRepo,
        IRepository<BikeImage> imageRepo,
        IRepository<AppUser> userRepo,
        IUnitOfWork uow)
    {
        _postRepo = postRepo;
        _imageRepo = imageRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<BikePostDto>> CreateAsync(Guid sellerId, CreateBikePostDto dto, CancellationToken ct = default)
    {
        var seller = await _userRepo.GetByIdAsync(sellerId, ct);
        if (seller is null) return Result<BikePostDto>.Failure("Seller not found");

        var post = new BikePost
        {
            SellerId = sellerId,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Brand = dto.Brand,
            Model = dto.Model,
            Year = dto.Year,
            Category = dto.Category,
            Size = dto.Size,
            FrameMaterial = dto.FrameMaterial,
            Color = dto.Color,
            Condition = dto.Condition,
            WeightKg = dto.WeightKg,
            OdometerKm = dto.OdometerKm,
            UsageHistory = dto.UsageHistory,
            HasAccidents = dto.HasAccidents,
            AccidentDescription = dto.AccidentDescription,
            City = dto.City,
            District = dto.District,
            Status = PostStatus.Draft
        };

        await _postRepo.AddAsync(post, ct);

        for (int i = 0; i < dto.ImageUrls.Count; i++)
        {
            await _imageRepo.AddAsync(new BikeImage
            {
                BikePostId = post.Id,
                ImageUrl = dto.ImageUrls[i],
                DisplayOrder = i,
                IsPrimary = i == 0
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<BikePostDto>.Success(MapToDto(post, seller));
    }

    public async Task<Result<BikePostDto>> UpdateAsync(Guid sellerId, UpdateBikePostDto dto, CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(dto.Id, ct);
        if (post is null) return Result<BikePostDto>.Failure("Post not found");
        if (post.SellerId != sellerId) return Result<BikePostDto>.Failure("You can only edit your own posts");

        post.Title = dto.Title;
        post.Description = dto.Description;
        post.Price = dto.Price;
        post.Brand = dto.Brand;
        post.Model = dto.Model;
        post.Year = dto.Year;
        post.Category = dto.Category;
        post.Size = dto.Size;
        post.FrameMaterial = dto.FrameMaterial;
        post.Color = dto.Color;
        post.Condition = dto.Condition;
        post.WeightKg = dto.WeightKg;
        post.OdometerKm = dto.OdometerKm;
        post.UsageHistory = dto.UsageHistory;
        post.HasAccidents = dto.HasAccidents;
        post.AccidentDescription = dto.AccidentDescription;
        post.City = dto.City;
        post.District = dto.District;

        _postRepo.Update(post);
        await _uow.SaveChangesAsync(ct);

        var seller = await _userRepo.GetByIdAsync(sellerId, ct);
        if (seller is null) return Result<BikePostDto>.Failure("Seller not found");
        return Result<BikePostDto>.Success(MapToDto(post, seller));
    }

    public async Task<Result> DeleteAsync(Guid sellerId, Guid postId, CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId, ct);
        if (post is null) return Result.Failure("Post not found");
        if (post.SellerId != sellerId) return Result.Failure("You can only delete your own posts");

        _postRepo.Delete(post);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ToggleVisibilityAsync(Guid sellerId, Guid postId, CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId, ct);
        if (post is null) return Result.Failure("Post not found");
        if (post.SellerId != sellerId) return Result.Failure("You can only modify your own posts");

        post.Status = post.Status == PostStatus.Hidden ? PostStatus.Active : PostStatus.Hidden;
        _postRepo.Update(post);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SubmitForModerationAsync(Guid sellerId, Guid postId, CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId, ct);
        if (post is null) return Result.Failure("Post not found");
        if (post.SellerId != sellerId) return Result.Failure("You can only submit your own posts");

        post.Status = PostStatus.PendingModeration;
        _postRepo.Update(post);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<BikePostDto>> GetByIdAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdWithIncludesAsync(postId, ct, p => p.Images);
        if (post is null) return Result<BikePostDto>.Failure("Post not found");

        var seller = await _userRepo.GetByIdAsync(post.SellerId, ct);
        if (seller is null) return Result<BikePostDto>.Failure("Seller not found");

        return Result<BikePostDto>.Success(MapToDto(post, seller));
    }

    public async Task<Result<List<BikePostDto>>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default)
    {
        var posts = await _postRepo.FindWithIncludesAsync(p => p.SellerId == sellerId, ct, p => p.Images);
        var seller = await _userRepo.GetByIdAsync(sellerId, ct);
        if (seller is null) return Result<List<BikePostDto>>.Failure("Seller not found");

        var dtos = posts.Select(p => MapToDto(p, seller)).ToList();
        return Result<List<BikePostDto>>.Success(dtos);
    }

    private static BikePostDto MapToDto(BikePost post, AppUser seller)
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
            SellerId = seller.Id,
            SellerName = seller.FullName,
            SellerAvatar = seller.AvatarUrl,
            SellerRating = seller.SellerRating,
            IsVerifiedSeller = seller.IsVerifiedSeller,
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
