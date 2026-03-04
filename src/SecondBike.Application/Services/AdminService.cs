using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Admin;
using SecondBike.Application.DTOs.Orders;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Admin Dashboard — Listing moderation, user management, and dispute resolution.
/// Business logic belongs in Application layer.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserRole> _roleRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderDetail> _orderDetailRepo;
    private readonly IRepository<ListingMedium> _mediaRepo;
    private readonly IRepository<RequestAbuse> _abuseRequestRepo;
    private readonly IBikeListingRepository _bikeListingRepo;
    private readonly IUnitOfWork _uow;

    public AdminService(
        IRepository<BicycleListing> listingRepo,
        IRepository<User> userRepo,
        IRepository<UserRole> roleRepo,
        IRepository<Order> orderRepo,
        IRepository<OrderDetail> orderDetailRepo,
        IRepository<ListingMedium> mediaRepo,
        IRepository<RequestAbuse> abuseRequestRepo,
        IBikeListingRepository bikeListingRepo,
        IUnitOfWork uow)
    {
        _listingRepo = listingRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _orderRepo = orderRepo;
        _orderDetailRepo = orderDetailRepo;
        _mediaRepo = mediaRepo;
        _abuseRequestRepo = abuseRequestRepo;
        _bikeListingRepo = bikeListingRepo;
        _uow = uow;
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var totalUsers = await _userRepo.CountAsync(cancellationToken: ct);
        var activeListings = await _listingRepo.CountAsync(l => l.ListingStatus == 1, ct);
        var pendingMods = await _listingRepo.CountAsync(l => l.ListingStatus == 2, ct);
        var totalOrders = await _orderRepo.CountAsync(cancellationToken: ct);

        var pendingAbuse = await _abuseRequestRepo.FindWithIncludesAsync(
            r => r.ReportAbuse == null, ct, r => r.ReportAbuse!);
        var pendingAbuseCount = pendingAbuse.Count;

        var completedOrders = await _orderRepo.FindAsync(o => o.OrderStatus == 4, ct);
        var totalRevenue = completedOrders.Sum(o => o.TotalAmount ?? 0);

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalActiveListings = activeListings,
            PendingModerations = pendingMods,
            PendingAbuseReports = pendingAbuseCount,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue
        });
    }

    public async Task<Result<List<PendingPostDto>>> GetPendingPostsAsync(CancellationToken ct = default)
    {
        var listings = await _listingRepo.FindAsync(l => l.ListingStatus == 2, ct);
        var dtos = new List<PendingPostDto>();

        foreach (var listing in listings.OrderBy(l => l.PostedDate))
        {
            var detail = await _bikeListingRepo.GetWithDetailsAsync(listing.ListingId, ct);
            var media = await _mediaRepo.FindAsync(m => m.ListingId == listing.ListingId && m.IsThumbnail == true, ct);

            dtos.Add(new PendingPostDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                SellerName = detail?.Seller?.Username ?? "Unknown",
                Price = listing.Price,
                BrandName = detail?.Bike?.Brand?.BrandName,
                TypeName = detail?.Bike?.Type?.TypeName,
                PostedDate = listing.PostedDate,
                PrimaryImageUrl = media.FirstOrDefault()?.MediaUrl
            });
        }

        return Result<List<PendingPostDto>>.Success(dtos);
    }

    public async Task<Result> ModeratePostAsync(int adminId, ModeratePostDto dto, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(dto.ListingId, ct);
        if (listing is null) return Result.Failure("Listing not found");
        if (listing.ListingStatus != 2) return Result.Failure("Listing is not pending moderation");

        listing.ListingStatus = dto.Approve ? (byte)1 : (byte)4;
        _listingRepo.Update(listing);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<List<AdminUserDto>>> GetUsersAsync(int? roleId = null, CancellationToken ct = default)
    {
        var users = roleId.HasValue
            ? await _userRepo.FindAsync(u => u.RoleId == roleId.Value, ct)
            : await _userRepo.GetAllAsync(ct);

        var dtos = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var role = await _roleRepo.GetByIdAsync(u.RoleId, ct);
            var listingCount = await _listingRepo.CountAsync(l => l.SellerId == u.UserId, ct);
            var orderCount = await _orderRepo.CountAsync(o => o.BuyerId == u.UserId, ct);

            dtos.Add(new AdminUserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                RoleName = role?.RoleName ?? "Unknown",
                Status = u.Status,
                CreatedAt = u.CreatedAt,
                TotalListings = listingCount,
                TotalOrders = orderCount
            });
        }

        return Result<List<AdminUserDto>>.Success(dtos);
    }

    public async Task<Result> UpdateUserStatusAsync(int adminId, int userId, byte status, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result.Failure("User not found");

        user.Status = status;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> ResolveDisputeAsync(int adminId, ResolveDisputeDto dto, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct);
        if (order is null) return Result.Failure("Order not found");

        order.OrderStatus = dto.RefundBuyer ? (byte)6 : (byte)4;
        _orderRepo.Update(order);

        if (dto.BanSeller)
        {
            var details = await _orderDetailRepo.FindAsync(d => d.OrderId == order.OrderId, ct);
            foreach (var detail in details)
            {
                var listing = await _listingRepo.GetByIdAsync(detail.ListingId, ct);
                if (listing is not null)
                {
                    var seller = await _userRepo.GetByIdAsync(listing.SellerId, ct);
                    if (seller is not null)
                    {
                        seller.Status = 0;
                        _userRepo.Update(seller);
                    }
                }
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
