using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Quality &amp; Auth — Registration, login (JWT-based), Google login, and profile management.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IRepository<AppUser> _appUserRepo;
    private readonly IRepository<Wallet> _walletRepo;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        IRepository<AppUser> appUserRepo,
        IRepository<Wallet> walletRepo,
        IUnitOfWork uow,
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _appUserRepo = appUserRepo;
        _walletRepo = walletRepo;
        _uow = uow;
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var identityUser = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(identityUser, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<AuthResultDto>.Failure(errors);
        }

        // Add role
        await _userManager.AddToRoleAsync(identityUser, dto.Role.ToString());

        // Create AppUser
        var appUser = new AppUser
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role,
            IdentityUserId = identityUser.Id
        };

        await _appUserRepo.AddAsync(appUser, ct);

        // Create wallet
        await _walletRepo.AddAsync(new Wallet
        {
            UserId = appUser.Id,
            Balance = 0
        }, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User registered: {Email}, Role: {Role}", dto.Email, dto.Role);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = _jwtService.GenerateToken(appUser),
            User = MapToDto(appUser)
        });
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        // 1. Find Identity user by email
        var identityUser = await _userManager.FindByEmailAsync(dto.Email);
        if (identityUser is null)
            return Result<AuthResultDto>.Failure("Invalid email or password");

        // 2. Check lockout
        if (await _userManager.IsLockedOutAsync(identityUser))
            return Result<AuthResultDto>.Failure("Account is locked. Please try again later.");

        // 3. Verify password directly (no cookie, pure JWT flow)
        var passwordValid = await _userManager.CheckPasswordAsync(identityUser, dto.Password);
        if (!passwordValid)
        {
            // Record failed attempt for lockout
            await _userManager.AccessFailedAsync(identityUser);
            return Result<AuthResultDto>.Failure("Invalid email or password");
        }

        // 4. Reset failed count on success
        await _userManager.ResetAccessFailedCountAsync(identityUser);

        // 5. Find AppUser profile
        var appUsers = await _appUserRepo.FindAsync(u => u.IdentityUserId == identityUser.Id, ct);
        var appUser = appUsers.FirstOrDefault();
        if (appUser is null)
            return Result<AuthResultDto>.Failure("User profile not found");

        // 6. Check if user is banned/suspended
        if (appUser.Status == Domain.Enums.UserStatus.Banned)
            return Result<AuthResultDto>.Failure("Your account has been banned");

        if (appUser.Status == Domain.Enums.UserStatus.Suspended)
            return Result<AuthResultDto>.Failure("Your account has been suspended");

        _logger.LogInformation("User logged in: {Email}", dto.Email);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = _jwtService.GenerateToken(appUser),
            User = MapToDto(appUser)
        });
    }

    public async Task<Result<AuthResultDto>> GoogleLoginAsync(GoogleLoginDto dto, CancellationToken ct = default)
    {
        // 1. Validate the Google ID token
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_configuration["Google:ClientId"]!]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return Result<AuthResultDto>.Failure("Invalid Google token");
        }

        var email = payload.Email;
        var fullName = payload.Name ?? email;
        var avatarUrl = payload.Picture;

        // 2. Find or create Identity user
        var identityUser = await _userManager.FindByEmailAsync(email);
        if (identityUser is null)
        {
            identityUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(identityUser);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return Result<AuthResultDto>.Failure(errors);
            }

            await _userManager.AddToRoleAsync(identityUser, UserRole.Buyer.ToString());
            await _userManager.AddLoginAsync(identityUser,
                new UserLoginInfo("Google", payload.Subject, "Google"));
        }

        // 3. Find or create AppUser profile
        var appUsers = await _appUserRepo.FindAsync(u => u.IdentityUserId == identityUser.Id, ct);
        var appUser = appUsers.FirstOrDefault();

        if (appUser is null)
        {
            appUser = new AppUser
            {
                Email = email,
                FullName = fullName,
                AvatarUrl = avatarUrl,
                Role = UserRole.Buyer,
                IdentityUserId = identityUser.Id
            };

            await _appUserRepo.AddAsync(appUser, ct);
            await _walletRepo.AddAsync(new Wallet { UserId = appUser.Id, Balance = 0 }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        // 4. Check account status
        if (appUser.Status == UserStatus.Banned)
            return Result<AuthResultDto>.Failure("Your account has been banned");

        if (appUser.Status == UserStatus.Suspended)
            return Result<AuthResultDto>.Failure("Your account has been suspended");

        _logger.LogInformation("Google login: {Email}", email);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = _jwtService.GenerateToken(appUser),
            User = MapToDto(appUser)
        });
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _appUserRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");
        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _appUserRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.AvatarUrl = dto.AvatarUrl;
        user.ShopName = dto.ShopName;
        user.ShopDescription = dto.ShopDescription;

        _appUserRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task LogoutAsync()
    {
        // JWT is stateless — logout is handled client-side by removing the token.
        // This method exists to satisfy the interface contract.
        await Task.CompletedTask;
    }

    private static UserProfileDto MapToDto(AppUser u)
    {
        return new UserProfileDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            PhoneNumber = u.PhoneNumber,
            AvatarUrl = u.AvatarUrl,
            Role = u.Role,
            ShopName = u.ShopName,
            ShopDescription = u.ShopDescription,
            IsVerifiedSeller = u.IsVerifiedSeller,
            SellerRating = u.SellerRating,
            TotalRatingsCount = u.TotalRatingsCount
        };
    }
}
