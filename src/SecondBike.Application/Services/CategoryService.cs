using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Categories;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Category (BikeType) management - CRUD operations for admin.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IRepository<BikeType> _typeRepo;
    private readonly IRepository<Bicycle> _bikeRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IRepository<BikeType> typeRepo,
        IRepository<Bicycle> bikeRepo,
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator,
        ILogger<CategoryService> logger)
    {
        _typeRepo = typeRepo;
        _bikeRepo = bikeRepo;
        _uow = uow;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var types = await _typeRepo.GetAllAsync(ct);

        // Batch-load bike counts to avoid N+1 queries
        var allBikes = await _bikeRepo.GetAllAsync(ct);
        var bikeCounts = allBikes.GroupBy(b => b.TypeId)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = types
            .OrderBy(t => t.TypeName)
            .Select(type => new CategoryDto
            {
                TypeId = type.TypeId,
                TypeName = type.TypeName,
                TotalBicycles = bikeCounts.GetValueOrDefault(type.TypeId, 0)
            })
            .ToList();

        return Result<List<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(int typeId, CancellationToken ct = default)
    {
        var type = await _typeRepo.GetByIdAsync(typeId, ct);
        if (type is null)
            return Result<CategoryDto>.Failure("Category not found");

        var bikeCount = await _bikeRepo.CountAsync(b => b.TypeId == type.TypeId, ct);

        return Result<CategoryDto>.Success(new CategoryDto
        {
            TypeId = type.TypeId,
            TypeName = type.TypeName,
            TotalBicycles = bikeCount
        });
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<CategoryDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var exists = await _typeRepo.AnyAsync(t => t.TypeName.ToLower() == dto.TypeName.Trim().ToLower(), ct);
        if (exists)
            return Result<CategoryDto>.Failure("Category name already exists");

        var entity = _mapper.Map<BikeType>(dto);
        entity.TypeName = dto.TypeName.Trim();

        await _typeRepo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Category {TypeId} ({TypeName}) created", entity.TypeId, entity.TypeName);

        return Result<CategoryDto>.Success(new CategoryDto
        {
            TypeId = entity.TypeId,
            TypeName = entity.TypeName,
            TotalBicycles = 0
        });
    }

    public async Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<CategoryDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _typeRepo.GetByIdAsync(dto.TypeId, ct);
        if (entity is null)
            return Result<CategoryDto>.Failure("Category not found");

        var exists = await _typeRepo.AnyAsync(
            t => t.TypeName.ToLower() == dto.TypeName.Trim().ToLower() && t.TypeId != dto.TypeId, ct);
        if (exists)
            return Result<CategoryDto>.Failure("Category name already exists");

        entity.TypeName = dto.TypeName.Trim();
        _typeRepo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Category {TypeId} updated to ({TypeName})", entity.TypeId, entity.TypeName);

        var bikeCount = await _bikeRepo.CountAsync(b => b.TypeId == entity.TypeId, ct);
        return Result<CategoryDto>.Success(new CategoryDto
        {
            TypeId = entity.TypeId,
            TypeName = entity.TypeName,
            TotalBicycles = bikeCount
        });
    }

    public async Task<Result> DeleteAsync(int typeId, CancellationToken ct = default)
    {
        var entity = await _typeRepo.GetByIdAsync(typeId, ct);
        if (entity is null)
            return Result.Failure("Category not found");

        var hasBikes = await _bikeRepo.AnyAsync(b => b.TypeId == typeId, ct);
        if (hasBikes)
            return Result.Failure("Cannot delete category that has associated bicycles");

        _typeRepo.Delete(entity);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Category {TypeId} ({TypeName}) deleted", typeId, entity.TypeName);

        return Result.Success();
    }
}