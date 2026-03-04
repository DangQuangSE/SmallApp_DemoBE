using AutoMapper;
using SecondBike.Application.DTOs.Brands;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Mappings;

public class BrandMappingProfile : Profile
{
    public BrandMappingProfile()
    {
        CreateMap<Brand, BrandDto>()
            .ForMember(d => d.TotalBicycles, o => o.MapFrom(s => s.Bicycles.Count));

        CreateMap<CreateBrandDto, Brand>()
            .ForMember(d => d.BrandId, o => o.Ignore())
            .ForMember(d => d.Bicycles, o => o.Ignore());
    }
}
