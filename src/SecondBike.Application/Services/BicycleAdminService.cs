using AutoMapper;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

public class BicycleAdminService : IBicycleAdminService
{
    private readonly IRepository<Bicycle> _bikeRepo;
    private readonly IRepository<BicycleDetail> _detailRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<BicycleAdminService> _logger;

    public BicycleAdminService(
        IRepository<Bicycle> bikeRepo,
        IRepository<BicycleDetail> detailRepo,
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<BicycleAdminService> logger)
    {
        _bikeRepo = bikeRepo;
        _detailRepo = detailRepo;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<int>> CreateBicycleAsync(CreateBicycleDto dto, CancellationToken ct = default)
    {
        var bike = new Bicycle
        {
            BrandId = dto.BrandId,
            TypeId = dto.TypeId,
            ModelName = dto.ModelName,
            SerialNumber = dto.SerialNumber,
            Color = dto.Color
        };

        await _bikeRepo.AddAsync(bike, ct);
        await _uow.SaveChangesAsync(ct);

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
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Admin created a new bicycle model securely: BikeId {BikeId}", bike.BikeId);

        return Result<int>.Success(bike.BikeId);
    }

    public async Task<Result<List<BicycleDto>>> GetAllBicyclesAsync(CancellationToken ct = default)
    {
        var bikes = await _bikeRepo.FindWithIncludesAsync(b => true, ct, 
            b => b.BicycleDetail, 
            b => b.Brand, 
            b => b.Type);

        var dtos = bikes.Select(b => new BicycleDto
        {
            BikeId = b.BikeId,
            BrandId = b.BrandId,
            BrandName = b.Brand?.BrandName,
            TypeId = b.TypeId,
            TypeName = b.Type?.TypeName,
            ModelName = b.ModelName,
            SerialNumber = b.SerialNumber,
            Color = b.Color,
            FrameSize = b.BicycleDetail?.FrameSize,
            FrameMaterial = b.BicycleDetail?.FrameMaterial,
            WheelSize = b.BicycleDetail?.WheelSize,
            BrakeType = b.BicycleDetail?.BrakeType,
            Weight = b.BicycleDetail?.Weight,
            Transmission = b.BicycleDetail?.Transmission
        }).ToList();

        return Result<List<BicycleDto>>.Success(dtos);
    }
}
