using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Admin;

public class ModeratePostDto
{
    public Guid BikePostId { get; set; }
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

public class PendingPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public BikeCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PrimaryImageUrl { get; set; }
}

public class ResolveDisputeDto
{
    public Guid OrderId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public bool RefundBuyer { get; set; }
    public bool BanSeller { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalPosts { get; set; }
    public int TotalOrders { get; set; }
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalActivePosts { get; set; }
    public int PendingModerations { get; set; }
    public int OpenDisputes { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
