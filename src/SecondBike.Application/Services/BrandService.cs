using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Brands;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Brand management - CRUD operations for admin.
/// </summary>
public class BrandService : IBrandService
{
    private readonly IRepository<Brand> _brandRepo;
    private readonly IRepository<Bicycle> _bikeRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateBrandDto> _createValidator;
    private readonly IValidator<UpdateBrandDto> _updateValidator;
    private readonly ILogger<BrandService> _logger;

    public BrandService(
        IRepository<Brand> brandRepo,
        IRepository<Bicycle> bikeRepo,
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<CreateBrandDto> createValidator,
        IValidator<UpdateBrandDto> updateValidator,
        ILogger<BrandService> logger)
    {
        _brandRepo = brandRepo;
        _bikeRepo = bikeRepo;
        _uow = uow;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<Result<List<BrandDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var brands = await _brandRepo.GetAllAsync(ct);

        // Batch-load bike counts to avoid N+1 queries
        var allBikes = await _bikeRepo.GetAllAsync(ct);
        var bikeCounts = allBikes
            .Where(b => b.BrandId.HasValue)
            .GroupBy(b => b.BrandId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = brands
            .OrderBy(b => b.BrandName)
            .Select(brand => new BrandDto
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                Country = brand.Country,
                TotalBicycles = bikeCounts.GetValueOrDefault(brand.BrandId, 0)
            })
            .ToList();

        return Result<List<BrandDto>>.Success(dtos);
    }

    public async Task<Result<BrandDto>> GetByIdAsync(int brandId, CancellationToken ct = default)
    {
        var brand = await _brandRepo.GetByIdAsync(brandId, ct);
        if (brand is null)
            return Result<BrandDto>.Failure("Brand not found");

        var bikeCount = await _bikeRepo.CountAsync(b => b.BrandId == brand.BrandId, ct);

        return Result<BrandDto>.Success(new BrandDto
        {
            BrandId = brand.BrandId,
            BrandName = brand.BrandName,
            Country = brand.Country,
            TotalBicycles = bikeCount
        });
    }

    public async Task<Result<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<BrandDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var exists = await _brandRepo.AnyAsync(b => b.BrandName.ToLower() == dto.BrandName.Trim().ToLower(), ct);
        if (exists)
            return Result<BrandDto>.Failure("Brand name already exists");

        var entity = _mapper.Map<Brand>(dto);
        entity.BrandName = dto.BrandName.Trim();
        entity.Country = dto.Country?.Trim();

        await _brandRepo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Brand {BrandId} ({BrandName}) created", entity.BrandId, entity.BrandName);

        return Result<BrandDto>.Success(new BrandDto
        {
            BrandId = entity.BrandId,
            BrandName = entity.BrandName,
            Country = entity.Country,
            TotalBicycles = 0
        });
    }

    public async Task<Result<BrandDto>> UpdateAsync(UpdateBrandDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<BrandDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _brandRepo.GetByIdAsync(dto.BrandId, ct);
        if (entity is null)
            return Result<BrandDto>.Failure("Brand not found");

        var exists = await _brandRepo.AnyAsync(
            b => b.BrandName.ToLower() == dto.BrandName.Trim().ToLower() && b.BrandId != dto.BrandId, ct);
        if (exists)
            return Result<BrandDto>.Failure("Brand name already exists");

        entity.BrandName = dto.BrandName.Trim();
        entity.Country = dto.Country?.Trim();
        _brandRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Brand {BrandId} updated to ({BrandName})", entity.BrandId, entity.BrandName);

        var bikeCount = await _bikeRepo.CountAsync(b => b.BrandId == entity.BrandId, ct);
        return Result<BrandDto>.Success(new BrandDto
        {
            BrandId = entity.BrandId,
            BrandName = entity.BrandName,
            Country = entity.Country,
            TotalBicycles = bikeCount
        });
    }

    public async Task<Result> DeleteAsync(int brandId, CancellationToken ct = default)
    {
        var entity = await _brandRepo.GetByIdAsync(brandId, ct);
        if (entity is null)
            return Result.Failure("Brand not found");

        var hasBikes = await _bikeRepo.AnyAsync(b => b.BrandId == brandId, ct);
        if (hasBikes)
            return Result.Failure("Cannot delete brand that has associated bicycles");

        _brandRepo.Delete(entity);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Brand {BrandId} ({BrandName}) deleted", brandId, entity.BrandName);

        return Result.Success();
    }
}