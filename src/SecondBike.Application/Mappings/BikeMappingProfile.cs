using AutoMapper;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between Bike-related entities and DTOs.
/// </summary>
public class BikeMappingProfile : Profile
{
    public BikeMappingProfile()
    {
        // ListingMedium -> BikeImageDto
        CreateMap<ListingMedium, BikeImageDto>();

        // BicycleListing -> BikePostDto (flattening from navigation properties)
        CreateMap<BicycleListing, BikePostDto>()
            .ForMember(d => d.ModelName, o => o.MapFrom(s => s.Bike.ModelName))
            .ForMember(d => d.SerialNumber, o => o.MapFrom(s => s.Bike.SerialNumber))
            .ForMember(d => d.Color, o => o.MapFrom(s => s.Bike.Color))
            .ForMember(d => d.Condition, o => o.MapFrom(s => s.Bike.Condition))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Bike.Brand != null ? s.Bike.Brand.BrandName : null))
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Bike.Type != null ? s.Bike.Type.TypeName : null))
            .ForMember(d => d.FrameSize, o => o.MapFrom(s => s.Bike.BicycleDetail != null ? s.Bike.BicycleDetail.FrameSize : null))
            .ForMember(d => d.FrameMaterial, o => o.MapFrom(s => s.Bike.BicycleDetail != null ? s.Bike.BicycleDetail.FrameMaterial : null))
            .ForMember(d => d.WheelSize, o => o.MapFrom(s => s.Bike.BicycleDetail != null ? s.Bike.BicycleDetail.WheelSize : null))
            .ForMember(d => d.BrakeType, o => o.MapFrom(s => s.Bike.BicycleDetail != null ? s.Bike.BicycleDetail.BrakeType : null))
            .ForMember(d => d.Weight, o => o.MapFrom(s => s.Bike.BicycleDetail != null ? s.Bike.BicycleDetail.Weight : (decimal?)null))
            .ForMember(d => d.Transmission, o => o.MapFrom(s => s.Bike.BicycleDetail != null ? s.Bike.BicycleDetail.Transmission : null))
            .ForMember(d => d.SellerName, o => o.MapFrom(s => s.Seller != null ? s.Seller.Username : "Unknown"))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.ListingMedia))
            .ForMember(d => d.HasInspection, o => o.MapFrom(s => s.InspectionRequests.Any()));

        // CreateBikePostDto -> Bicycle
        CreateMap<CreateBikePostDto, Bicycle>()
            .ForMember(d => d.BikeId, o => o.Ignore())
            .ForMember(d => d.BicycleDetail, o => o.Ignore())
            .ForMember(d => d.BicycleListings, o => o.Ignore())
            .ForMember(d => d.Brand, o => o.Ignore())
            .ForMember(d => d.Type, o => o.Ignore());

        // CreateBikePostDto -> BicycleDetail
        CreateMap<CreateBikePostDto, BicycleDetail>()
            .ForMember(d => d.DetailId, o => o.Ignore())
            .ForMember(d => d.BikeId, o => o.Ignore())
            .ForMember(d => d.Bike, o => o.Ignore());

        // CreateBikePostDto -> BicycleListing
        CreateMap<CreateBikePostDto, BicycleListing>()
            .ForMember(d => d.ListingId, o => o.Ignore())
            .ForMember(d => d.SellerId, o => o.Ignore())
            .ForMember(d => d.BikeId, o => o.Ignore())
            .ForMember(d => d.ListingStatus, o => o.Ignore())
            .ForMember(d => d.PostedDate, o => o.Ignore())
            .ForMember(d => d.Bike, o => o.Ignore())
            .ForMember(d => d.Seller, o => o.Ignore())
            .ForMember(d => d.ListingMedia, o => o.Ignore())
            .ForMember(d => d.ChatSessions, o => o.Ignore())
            .ForMember(d => d.InspectionRequests, o => o.Ignore())
            .ForMember(d => d.OrderDetails, o => o.Ignore())
            .ForMember(d => d.RequestAbuses, o => o.Ignore())
            .ForMember(d => d.ShoppingCarts, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());

        // UpdateBikePostDto -> BicycleListing (update existing entity)
        CreateMap<UpdateBikePostDto, BicycleListing>()
            .ForMember(d => d.ListingId, o => o.Ignore())
            .ForMember(d => d.SellerId, o => o.Ignore())
            .ForMember(d => d.BikeId, o => o.Ignore())
            .ForMember(d => d.ListingStatus, o => o.Ignore())
            .ForMember(d => d.PostedDate, o => o.Ignore())
            .ForMember(d => d.Bike, o => o.Ignore())
            .ForMember(d => d.Seller, o => o.Ignore())
            .ForMember(d => d.ListingMedia, o => o.Ignore())
            .ForMember(d => d.ChatSessions, o => o.Ignore())
            .ForMember(d => d.InspectionRequests, o => o.Ignore())
            .ForMember(d => d.OrderDetails, o => o.Ignore())
            .ForMember(d => d.RequestAbuses, o => o.Ignore())
            .ForMember(d => d.ShoppingCarts, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());

        // UpdateBikePostDto -> Bicycle (update existing entity)
        CreateMap<UpdateBikePostDto, Bicycle>()
            .ForMember(d => d.BikeId, o => o.Ignore())
            .ForMember(d => d.BicycleDetail, o => o.Ignore())
            .ForMember(d => d.BicycleListings, o => o.Ignore())
            .ForMember(d => d.Brand, o => o.Ignore())
            .ForMember(d => d.Type, o => o.Ignore());

        // UpdateBikePostDto -> BicycleDetail (update existing entity)
        CreateMap<UpdateBikePostDto, BicycleDetail>()
            .ForMember(d => d.DetailId, o => o.Ignore())
            .ForMember(d => d.BikeId, o => o.Ignore())
            .ForMember(d => d.Bike, o => o.Ignore());
    }
}
