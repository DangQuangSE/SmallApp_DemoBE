using AutoMapper;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Buyer Experience — Search and filter bicycle listings.
/// Business logic belongs in Application layer.
/// Uses IBikeListingRepository for complex EF queries.
/// </summary>
public class BikeSearchService : IBikeSearchService
{
    private readonly IBikeListingRepository _bikeListingRepo;
    private readonly IRepository<Brand> _brandRepo;
    private readonly IRepository<BikeType> _typeRepo;
    private readonly IMapper _mapper;

    public BikeSearchService(
        IBikeListingRepository bikeListingRepo,
        IRepository<Brand> brandRepo,
        IRepository<BikeType> typeRepo,
        IMapper mapper)
    {
        _bikeListingRepo = bikeListingRepo;
        _brandRepo = brandRepo;
        _typeRepo = typeRepo;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<BikePostDto>>> SearchAsync(BikeFilterDto filter, CancellationToken ct = default)
    {
        var (items, totalCount) = await _bikeListingRepo.SearchAsync(filter, ct);
        var dtos = _mapper.Map<List<BikePostDto>>(items);

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
        var listing = await _bikeListingRepo.GetWithDetailsAsync(listingId, ct);
        if (listing is null)
            return Result<BikePostDto>.Failure("Listing not found");

        return Result<BikePostDto>.Success(_mapper.Map<BikePostDto>(listing));
    }

    public async Task<Result<List<string>>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = await _brandRepo.GetAllAsync(ct);
        var names = brands.Select(b => b.BrandName)
            .Distinct()
            .OrderBy(b => b)
            .ToList();

        return Result<List<string>>.Success(names);
    }

    public async Task<Result<List<string>>> GetTypesAsync(CancellationToken ct = default)
    {
        var types = await _typeRepo.GetAllAsync(ct);
        var names = types.Select(t => t.TypeName)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return Result<List<string>>.Success(names);
    }
}
