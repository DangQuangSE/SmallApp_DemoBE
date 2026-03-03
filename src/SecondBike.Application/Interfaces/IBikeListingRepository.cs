using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Interfaces;

/// <summary>
/// Specialized repository for BicycleListing queries that require
/// navigation property includes (Bike, Brand, Seller, Media, etc.).
/// Keeps DbContext/EF Core details out of the Application layer.
/// </summary>
public interface IBikeListingRepository
{
    /// <summary>
    /// Gets a single listing with all navigation properties loaded.
    /// </summary>
    Task<BicycleListing?> GetWithDetailsAsync(int listingId, CancellationToken ct = default);

    /// <summary>
    /// Gets all listings for a seller with all navigation properties loaded.
    /// </summary>
    Task<List<BicycleListing>> GetBySellerWithDetailsAsync(int sellerId, CancellationToken ct = default);

    /// <summary>
    /// Searches and filters listings with pagination.
    /// </summary>
    Task<(List<BicycleListing> Items, int TotalCount)> SearchAsync(BikeFilterDto filter, CancellationToken ct = default);

    /// <summary>
    /// Checks if any OrderDetail references the given listing.
    /// </summary>
    Task<bool> HasOrderDetailsAsync(int listingId, CancellationToken ct = default);
}
