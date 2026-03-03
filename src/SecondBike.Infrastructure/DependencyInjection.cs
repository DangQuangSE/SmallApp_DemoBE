using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Infrastructure.Repositories;
using SecondBike.Infrastructure.Services;

namespace SecondBike.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services including EF Core, Identity, repositories, and application services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Entity Framework Core with SQL Server
        services.AddDbContext<SecondBikeDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Generic Repository and Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Application Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IImageStorageService, CloudinaryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IBikePostService, BikePostService>();
        services.AddScoped<IBikeSearchService, BikeSearchService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddSingleton<IVnPayService, VnPayService>();

        return services;
    }
}
