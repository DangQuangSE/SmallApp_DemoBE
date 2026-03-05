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
/// Only external/infra concerns: EF Core, repositories, and third-party integrations.
/// Business services are registered in Application.DependencyInjection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<SecondBikeDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBikeListingRepository, BikeListingRepository>();

        // External / Infrastructure services only
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IImageStorageService, CloudinaryService>();
        services.AddSingleton<IVnPayService, VnPayService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidatorService>();

        return services;
    }
}
