namespace SecondBike.Application.DTOs.UserManagement;

public class UserManagementDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public byte? Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool? IsVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public int TotalListings { get; set; }
    public int TotalOrders { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class UpdateUserDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public byte? Status { get; set; }
    public bool? IsVerified { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public int? RoleId { get; set; }
    public byte? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

