using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Authentication — Registration with email confirmation, login, and profile management.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserProfile> _profileRepo;
    private readonly IRepository<UserRole> _roleRepo;
    private readonly IRepository<RefreshToken> _tokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepo,
        IRepository<UserProfile> profileRepo,
        IRepository<UserRole> roleRepo,
        IRepository<RefreshToken> tokenRepo,
        IUnitOfWork uow,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _roleRepo = roleRepo;
        _tokenRepo = tokenRepo;
        _uow = uow;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var existingByEmail = await _userRepo.FindAsync(u => u.Email == dto.Email, ct);
        if (existingByEmail.Count > 0)
            return Result<AuthResultDto>.Failure("Email already registered");

        var existingByUsername = await _userRepo.FindAsync(u => u.Username == dto.Username, ct);
        if (existingByUsername.Count > 0)
            return Result<AuthResultDto>.Failure("Username already taken");

        var role = await _roleRepo.GetByIdAsync(dto.RoleId, ct);
        if (role is null)
            return Result<AuthResultDto>.Failure("Invalid role");

        // Create user with IsVerified = false
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            IsVerified = false,
            Status = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        // Create user profile
        var profile = new UserProfile
        {
            UserId = user.UserId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber
        };
        await _profileRepo.AddAsync(profile, ct);
        await _uow.SaveChangesAsync(ct);

        // Generate email confirmation token & send email
        await SendConfirmationEmailAsync(user, ct);

        _logger.LogInformation("User registered: {Email}, Role: {Role}, awaiting email confirmation", dto.Email, role.RoleName);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = null, // No JWT until email is confirmed
            User = MapToDto(user, profile, role.RoleName),
            RequiresEmailConfirmation = true
        });
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var users = await _userRepo.FindAsync(u => u.Email == dto.Email, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return Result<AuthResultDto>.Failure("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResultDto>.Failure("Invalid email or password");

        if (user.Status == 0)
            return Result<AuthResultDto>.Failure("Your account has been banned");

        // Block login if email not confirmed
        if (user.IsVerified != true)
        {
            return Result<AuthResultDto>.Success(new AuthResultDto
            {
                Succeeded = false,
                Token = null,
                ErrorMessage = "Please verify your email before logging in",
                RequiresEmailConfirmation = true,
                User = new UserProfileDto { UserId = user.UserId, Email = user.Email, Username = user.Username }
            });
        }

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var profiles = await _profileRepo.FindAsync(p => p.UserId == user.UserId, ct);
        var profile = profiles.FirstOrDefault();

        _logger.LogInformation("User logged in: {Email}", dto.Email);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = _jwtService.GenerateToken(user, role?.RoleName ?? "Buyer"),
            User = MapToDto(user, profile, role?.RoleName ?? "Buyer")
        });
    }

    public async Task<Result<AuthResultDto>> GoogleLoginAsync(GoogleLoginDto dto, CancellationToken ct = default)
    {
        return Result<AuthResultDto>.Failure("Google login is not configured for this deployment");
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var profiles = await _profileRepo.FindAsync(p => p.UserId == userId, ct);
        var profile = profiles.FirstOrDefault();

        return Result<UserProfileDto>.Success(MapToDto(user, profile, role?.RoleName ?? "Buyer"));
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");

        var profiles = await _profileRepo.FindAsync(p => p.UserId == userId, ct);
        var profile = profiles.FirstOrDefault();

        if (profile is null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                AvatarUrl = dto.AvatarUrl,
                Address = dto.Address
            };
            await _profileRepo.AddAsync(profile, ct);
        }
        else
        {
            if (dto.FullName is not null) profile.FullName = dto.FullName;
            if (dto.PhoneNumber is not null) profile.PhoneNumber = dto.PhoneNumber;
            if (dto.AvatarUrl is not null) profile.AvatarUrl = dto.AvatarUrl;
            if (dto.Address is not null) profile.Address = dto.Address;
            _profileRepo.Update(profile);
        }

        await _uow.SaveChangesAsync(ct);

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        return Result<UserProfileDto>.Success(MapToDto(user, profile, role?.RoleName ?? "Buyer"));
    }

    public async Task<Result> ConfirmEmailAsync(string email, string token, CancellationToken ct = default)
    {
        var users = await _userRepo.FindAsync(u => u.Email == email, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return Result.Failure("User not found");

        if (user.IsVerified == true)
            return Result.Failure("Email is already confirmed");

        // Find a valid, non-expired, non-revoked token for this user
        var tokens = await _tokenRepo.FindAsync(t =>
            t.UserId == user.UserId &&
            t.Token == token &&
            t.RevokedAt == null, ct);
        var confirmToken = tokens.FirstOrDefault();

        if (confirmToken is null)
            return Result.Failure("Invalid confirmation token");

        if (confirmToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure("Confirmation token has expired. Please request a new one");

        // Mark user as verified
        user.IsVerified = true;
        _userRepo.Update(user);

        // Revoke the token so it can't be reused
        confirmToken.RevokedAt = DateTime.UtcNow;
        _tokenRepo.Update(confirmToken);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Email confirmed for user: {Email}", email);
        return Result.Success();
    }

    public async Task<Result> ResendConfirmationEmailAsync(string email, CancellationToken ct = default)
    {
        var users = await _userRepo.FindAsync(u => u.Email == email, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return Result.Failure("User not found");

        if (user.IsVerified == true)
            return Result.Failure("Email is already confirmed");

        // Revoke all existing confirmation tokens for this user
        var existingTokens = await _tokenRepo.FindAsync(t =>
            t.UserId == user.UserId && t.RevokedAt == null, ct);
        foreach (var t in existingTokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            _tokenRepo.Update(t);
        }

        await SendConfirmationEmailAsync(user, ct);

        _logger.LogInformation("Confirmation email resent to: {Email}", email);
        return Result.Success();
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    // ──────────── Private helpers ────────────

    private async Task SendConfirmationEmailAsync(User user, CancellationToken ct)
    {
        // Generate a secure random token
        var tokenValue = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var refreshToken = new RefreshToken
        {
            UserId = user.UserId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };
        await _tokenRepo.AddAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        // Build confirmation URL
        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5173";
        var confirmUrl = $"{baseUrl}/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={tokenValue}";

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2563eb;'>Xác nhận email — SecondBike</h2>
                <p>Xin chào <strong>{user.Username}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản SecondBike. Vui lòng nhấn nút bên dưới để xác nhận email:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{confirmUrl}'
                       style='background-color: #2563eb; color: white; padding: 12px 30px;
                              text-decoration: none; border-radius: 6px; font-size: 16px;'>
                        Xác nhận Email
                    </a>
                </div>
                <p style='color: #666; font-size: 14px;'>Link này sẽ hết hạn sau <strong>24 giờ</strong>.</p>
                <p style='color: #666; font-size: 14px;'>Nếu bạn không đăng ký tài khoản, vui lòng bỏ qua email này.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px;'>© SecondBike — Nền tảng mua bán xe đạp cũ</p>
            </div>";

        await _emailService.SendEmailAsync(user.Email, "Xác nhận email — SecondBike", htmlBody);
    }

    private static UserProfileDto MapToDto(User user, UserProfile? profile, string roleName)
    {
        return new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = profile?.FullName,
            PhoneNumber = profile?.PhoneNumber,
            AvatarUrl = profile?.AvatarUrl,
            Address = profile?.Address,
            RoleName = roleName,
            Status = user.Status,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }
}
