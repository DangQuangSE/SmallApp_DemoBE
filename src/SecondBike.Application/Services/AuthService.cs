using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Authentication — registration, login, OTP verification.
/// Business/orchestration logic belongs in Application layer.
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
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IGoogleTokenValidator _googleValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepo,
        IRepository<UserProfile> profileRepo,
        IRepository<UserRole> roleRepo,
        IRepository<RefreshToken> tokenRepo,
        IUnitOfWork uow,
        IJwtService jwtService,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IGoogleTokenValidator googleValidator,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _roleRepo = roleRepo;
        _tokenRepo = tokenRepo;
        _uow = uow;
        _jwtService = jwtService;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _googleValidator = googleValidator;
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

        var profile = new UserProfile
        {
            UserId = user.UserId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber
        };
        await _profileRepo.AddAsync(profile, ct);
        await _uow.SaveChangesAsync(ct);

        await SendOtpEmailAsync(user, ct);

        _logger.LogInformation("User registered: {Email}, Role: {Role}, awaiting OTP verification", dto.Email, role.RoleName);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = null,
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

        if (user.Status == 3)
            return Result<AuthResultDto>.Failure("Your account has been banned");

        if (user.Status == 0) // UserStatus.Deleted
            return Result<AuthResultDto>.Failure("Your account has been deleted");

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
        // Step 1: Validate Google ID token
        var googleUser = await _googleValidator.ValidateAsync(dto.IdToken, ct);
        if (googleUser is null)
            return Result<AuthResultDto>.Failure("Invalid or expired Google token");

        // Step 2: Security check - Only allow verified Google emails
        if (!googleUser.EmailVerified)
            return Result<AuthResultDto>.Failure("Google email is not verified");

        // Step 3: Find existing user or create new one
        var existingUsers = await _userRepo.FindAsync(u => u.Email == googleUser.Email, ct);
        var user = existingUsers.FirstOrDefault();

        if (user is not null)
        {
            // Existing user - check if account is active
            if (user.Status == 3)
                return Result<AuthResultDto>.Failure("Your account has been banned");

            if (user.Status == 0) // UserStatus.Deleted
                return Result<AuthResultDto>.Failure("Your account has been deleted");

            // Auto-verify user authenticated via Google
            if (user.IsVerified != true)
            {
                user.IsVerified = true;
                _userRepo.Update(user);
                await _uow.SaveChangesAsync(ct);
            }
        }
        else
        {
            // New user - Auto-register with Google account
            // Default role: Buyer (RoleId = 2)
            var buyerRole = await _roleRepo.GetByIdAsync(2, ct);
            if (buyerRole is null)
                return Result<AuthResultDto>.Failure("Default buyer role not found. Please contact administrator.");

            user = new User
            {
                Username = GenerateUsernameFromEmail(googleUser.Email),
                Email = googleUser.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password for OAuth users
                RoleId = 2, // Buyer
                IsVerified = true, // Google emails are pre-verified
                Status = 1, // Active
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            // Create user profile from Google data
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = googleUser.Name,
                AvatarUrl = googleUser.Picture
            };
            await _profileRepo.AddAsync(profile, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("New user auto-registered via Google: {Email}", googleUser.Email);
        }

        // Step 4: Load user profile and role for DTO mapping
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var profiles = await _profileRepo.FindAsync(p => p.UserId == user.UserId, ct);
        var userProfile = profiles.FirstOrDefault();

        _logger.LogInformation("User logged in via Google: {Email}", googleUser.Email);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = _jwtService.GenerateToken(user, role?.RoleName ?? "Buyer"),
            User = MapToDto(user, userProfile, role?.RoleName ?? "Buyer")
        });
    }

    /// <summary>
    /// Generate unique username from email for Google OAuth users.
    /// Format: prefix from email + random suffix to avoid collisions.
    /// </summary>
    private string GenerateUsernameFromEmail(string email)
    {
        var prefix = email.Split('@')[0];
        var randomSuffix = Random.Shared.Next(1000, 9999);
        return $"{prefix}_{randomSuffix}";
    }

    public async Task<Result> ConfirmEmailAsync(string email, string otp, CancellationToken ct = default)
    {
        var users = await _userRepo.FindAsync(u => u.Email == email, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return Result.Failure("User not found");

        if (user.IsVerified == true)
            return Result.Failure("Email is already confirmed");

        var tokens = await _tokenRepo.FindAsync(t =>
            t.UserId == user.UserId &&
            t.Token == otp &&
            t.RevokedAt == null, ct);
        var otpToken = tokens.FirstOrDefault();

        if (otpToken is null)
            return Result.Failure("Invalid verification code");

        if (otpToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure("Verification code has expired. Please request a new one");

        user.IsVerified = true;
        _userRepo.Update(user);

        otpToken.RevokedAt = DateTime.UtcNow;
        _tokenRepo.Update(otpToken);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Email confirmed via OTP for user: {Email}", email);
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

        var existingTokens = await _tokenRepo.FindAsync(t =>
            t.UserId == user.UserId && t.RevokedAt == null, ct);
        foreach (var t in existingTokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            _tokenRepo.Update(t);
        }

        await SendOtpEmailAsync(user, ct);

        _logger.LogInformation("OTP resent to: {Email}", email);
        return Result.Success();
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SendOtpEmailAsync(User user, CancellationToken ct)
    {
        var otpCode = Random.Shared.Next(100000, 999999).ToString();

        var refreshToken = new RefreshToken
        {
            UserId = user.UserId,
            Token = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        await _tokenRepo.AddAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        var (subject, body) = _emailTemplateService.BuildOtpEmail(user.Username, otpCode);
        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

    internal static UserProfileDto MapToDto(User user, UserProfile? profile, string roleName)
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
