using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Infrastructure.Data;
using SecondBike.Infrastructure.Repositories;
using SecondBike.Infrastructure.Services;

namespace SecondBike.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// in the dependency injection container.
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
        // Register Entity Framework Core with SQL Server using Factory
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Also register the scoped context for services that still inject it directly
        services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        // Register ASP.NET Core Identity
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Register Generic Repository and Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Application Services
        services.AddScoped<IBikePostService, BikePostService>();
        services.AddScoped<IBikeSearchService, BikeSearchService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
