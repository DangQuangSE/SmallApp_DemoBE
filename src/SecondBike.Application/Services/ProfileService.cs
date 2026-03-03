using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Users;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// User profile management — view, update, avatar upload, change password.
/// Business logic belongs in Application layer.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserProfile> _profileRepo;
    private readonly IRepository<UserRole> _roleRepo;
    private readonly IUnitOfWork _uow;
    private readonly IImageStorageService _imageStorage;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        IRepository<User> userRepo,
        IRepository<UserProfile> profileRepo,
        IRepository<UserRole> roleRepo,
        IUnitOfWork uow,
        IImageStorageService imageStorage,
        ILogger<ProfileService> logger)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _roleRepo = roleRepo;
        _uow = uow;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var profiles = await _profileRepo.FindAsync(p => p.UserId == userId, ct);
        var profile = profiles.FirstOrDefault();

        return Result<UserProfileDto>.Success(AuthService.MapToDto(user, profile, role?.RoleName ?? "Buyer"));
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
                Address = dto.Address
            };
            await _profileRepo.AddAsync(profile, ct);
        }
        else
        {
            if (dto.FullName is not null) profile.FullName = dto.FullName;
            if (dto.PhoneNumber is not null) profile.PhoneNumber = dto.PhoneNumber;
            if (dto.Address is not null) profile.Address = dto.Address;
            _profileRepo.Update(profile);
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Profile updated for user {UserId}", userId);

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        return Result<UserProfileDto>.Success(AuthService.MapToDto(user, profile, role?.RoleName ?? "Buyer"));
    }

    public async Task<Result<UserProfileDto>> UpdateAvatarAsync(int userId, Stream imageStream, string fileName, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");

        var profiles = await _profileRepo.FindAsync(p => p.UserId == userId, ct);
        var profile = profiles.FirstOrDefault();

        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            await _profileRepo.AddAsync(profile, ct);
            await _uow.SaveChangesAsync(ct);
        }

        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            await _imageStorage.DeleteAsync(profile.AvatarUrl);
        }

        var avatarUrl = await _imageStorage.UploadAsync(imageStream, fileName, "avatars");
        profile.AvatarUrl = avatarUrl;
        _profileRepo.Update(profile);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Avatar updated for user {UserId}", userId);

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        return Result<UserProfileDto>.Success(AuthService.MapToDto(user, profile, role?.RoleName ?? "Buyer"));
    }

    public async Task<Result<UserProfileDto>> RemoveAvatarAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result<UserProfileDto>.Failure("User not found");

        var profiles = await _profileRepo.FindAsync(p => p.UserId == userId, ct);
        var profile = profiles.FirstOrDefault();

        if (profile is null || string.IsNullOrEmpty(profile.AvatarUrl))
            return Result<UserProfileDto>.Failure("No avatar to remove");

        await _imageStorage.DeleteAsync(profile.AvatarUrl);
        profile.AvatarUrl = null;
        _profileRepo.Update(profile);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Avatar removed for user {UserId}", userId);

        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        return Result<UserProfileDto>.Success(AuthService.MapToDto(user, profile, role?.RoleName ?? "Buyer"));
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return Result.Failure("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return Result.Failure("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return Result.Success();
    }
}
