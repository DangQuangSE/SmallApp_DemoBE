using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Users;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public bool IsVerifiedSeller { get; set; }
    public decimal SellerRating { get; set; }
    public int TotalRatingsCount { get; set; }
}

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Buyer;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginDto
{
    public string IdToken { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public UserProfileDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}
