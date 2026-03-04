using AutoMapper;
using SecondBike.Application.DTOs.Categories;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Mappings;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<BikeType, CategoryDto>()
            .ForMember(d => d.TotalBicycles, o => o.MapFrom(s => s.Bicycles.Count));

        CreateMap<CreateCategoryDto, BikeType>()
            .ForMember(d => d.TypeId, o => o.Ignore())
            .ForMember(d => d.Bicycles, o => o.Ignore());
    }
}
