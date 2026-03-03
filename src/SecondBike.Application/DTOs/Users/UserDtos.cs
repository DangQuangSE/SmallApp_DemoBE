namespace SecondBike.Application.DTOs.Users;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Address { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public byte? Status { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Address { get; set; }
}

public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; } = 2; // Default Buyer
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
    public bool RequiresEmailConfirmation { get; set; }
}

public class ConfirmEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ResendConfirmationDto
{
    public string Email { get; set; } = string.Empty;
}
