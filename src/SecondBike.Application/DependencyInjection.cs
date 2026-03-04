using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Application.Services;

namespace SecondBike.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Business / Use-case services (Application layer)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IBikePostService, BikePostService>();
        services.AddScoped<IBikeSearchService, BikeSearchService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IUserManagerService, UserManagerService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();

        return services;
    }
}
