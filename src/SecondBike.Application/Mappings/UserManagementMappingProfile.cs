using AutoMapper;
using SecondBike.Application.DTOs.UserManagement;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Mappings;

public class UserManagementMappingProfile : Profile
{
    public UserManagementMappingProfile()
    {
        CreateMap<User, UserManagementDto>()
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.RoleName : "Unknown"))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.FullName : null))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.PhoneNumber : null))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.Address : null))
            .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.AvatarUrl : null))
            .ForMember(d => d.StatusName, o => o.Ignore())
            .ForMember(d => d.TotalListings, o => o.Ignore())
            .ForMember(d => d.TotalOrders, o => o.Ignore());

        CreateMap<CreateUserDto, User>()
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.IsVerified, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.UserProfile, o => o.Ignore())
            .ForMember(d => d.AuditLogSystems, o => o.Ignore())
            .ForMember(d => d.BicycleListings, o => o.Ignore())
            .ForMember(d => d.ChatMessages, o => o.Ignore())
            .ForMember(d => d.ChatSessionBuyers, o => o.Ignore())
            .ForMember(d => d.ChatSessionSellers, o => o.Ignore())
            .ForMember(d => d.FeedbackTargetUsers, o => o.Ignore())
            .ForMember(d => d.FeedbackUsers, o => o.Ignore())
            .ForMember(d => d.InspectionRequests, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.RefreshTokens, o => o.Ignore())
            .ForMember(d => d.ReportAbuses, o => o.Ignore())
            .ForMember(d => d.RequestAbuseReporters, o => o.Ignore())
            .ForMember(d => d.RequestAbuseTargetUsers, o => o.Ignore())
            .ForMember(d => d.ShoppingCarts, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());
    }
}
