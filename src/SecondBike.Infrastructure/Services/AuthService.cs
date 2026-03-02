using Microsoft.AspNetCore.Identity;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Quality & Auth � Registration, login, and profile management.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IRepository<AppUser> _appUserRepo;
    private readonly IRepository<Wallet> _walletRepo;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwtService;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IRepository<AppUser> appUserRepo,
        IRepository<Wallet> walletRepo,
        IUnitOfWork uow,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _appUserRepo = appUserRepo;
        _walletRepo = walletRepo;
        _uow = uow;
        _jwtService = jwtService;
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

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            Succeeded = true,
            Token = _jwtService.GenerateToken(appUser),
            User = MapToDto(appUser)
        });
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Result<AuthResultDto>.Failure("Invalid email or password");
        }

        var identityUser = await _userManager.FindByEmailAsync(dto.Email);
        if (identityUser is null)
            return Result<AuthResultDto>.Failure("User not found");

        var appUsers = await _appUserRepo.FindAsync(u => u.IdentityUserId == identityUser.Id, ct);
        var appUser = appUsers.FirstOrDefault();
        if (appUser is null)
            return Result<AuthResultDto>.Failure("User profile not found");

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
        await _signInManager.SignOutAsync();
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
