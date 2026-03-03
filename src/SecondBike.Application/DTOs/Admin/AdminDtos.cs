namespace SecondBike.Application.DTOs.Admin;

public class ModeratePostDto
{
    public int ListingId { get; set; }
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

public class PendingPostDto
{
    public int ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? BrandName { get; set; }
    public string? TypeName { get; set; }
    public DateTime? PostedDate { get; set; }
    public string? PrimaryImageUrl { get; set; }
}

public class ResolveDisputeDto
{
    public int OrderId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public bool RefundBuyer { get; set; }
    public bool BanSeller { get; set; }
}

public class AdminUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public byte? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int TotalListings { get; set; }
    public int TotalOrders { get; set; }
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalActiveListings { get; set; }
    public int PendingModerations { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
