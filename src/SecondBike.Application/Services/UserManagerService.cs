using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Common;
using SecondBike.Application.DTOs.UserManagement;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Application.Services;

/// <summary>
/// User management service - CRUD, search, and status operations for admin users.
/// </summary>
public class UserManagerService : IUserManagerService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserProfile> _profileRepo;
    private readonly IRepository<Domain.Entities.UserRole> _roleRepo;
    private readonly IRepository<BicycleListing> _listingRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateUserDto> _createValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly ILogger<UserManagerService> _logger;

    public UserManagerService(
        IRepository<User> userRepo,
        IRepository<UserProfile> profileRepo,
        IRepository<Domain.Entities.UserRole> roleRepo,
        IRepository<BicycleListing> listingRepo,
        IRepository<Order> orderRepo,
        IUnitOfWork uow,
        IMapper mapper,
        IValidator<CreateUserDto> createValidator,
        IValidator<UpdateUserDto> updateValidator,
        ILogger<UserManagerService> logger)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _roleRepo = roleRepo;
        _listingRepo = listingRepo;
        _orderRepo = orderRepo;
        _uow = uow;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<UserManagementDto>>> GetUsersAsync(UserFilterDto filter, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var query = allUsers.AsQueryable();

        if (filter.RoleId.HasValue)
            query = query.Where(u => u.RoleId == filter.RoleId.Value);

        if (filter.Status.HasValue)
            query = query.Where(u => u.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var users = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        // Batch-load related data to avoid N+1 queries
        var userIds = users.Select(u => u.UserId).ToList();

        var roles = await _roleRepo.GetAllAsync(ct);
        var roleLookup = roles.ToDictionary(r => r.RoleId, r => r.RoleName);

        var profiles = await _profileRepo.FindAsync(p => userIds.Contains(p.UserId), ct);
        var profileLookup = profiles.ToDictionary(p => p.UserId);

        var allListings = await _listingRepo.FindAsync(l => userIds.Contains(l.SellerId), ct);
        var listingCounts = allListings.GroupBy(l => l.SellerId)
            .ToDictionary(g => g.Key, g => g.Count());

        var allOrders = await _orderRepo.FindAsync(o => userIds.Contains(o.BuyerId), ct);
        var orderCounts = allOrders.GroupBy(o => o.BuyerId)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = users.Select(user => BuildUserDto(
            user, roleLookup, profileLookup, listingCounts, orderCounts
        )).ToList();

        var result = new PagedResult<UserManagementDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return Result<PagedResult<UserManagementDto>>.Success(result);
    }

    public async Task<Result<UserManagementDto>> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null)
            return Result<UserManagementDto>.Failure("User not found");

        var dto = await BuildUserDtoAsync(user, ct);
        return Result<UserManagementDto>.Success(dto);
    }

    public async Task<Result<UserManagementDto>> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<UserManagementDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var existsEmail = await _userRepo.AnyAsync(u => u.Email == dto.Email, ct);
        if (existsEmail)
            return Result<UserManagementDto>.Failure("Email already exists");

        var existsUsername = await _userRepo.AnyAsync(u => u.Username == dto.Username, ct);
        if (existsUsername)
            return Result<UserManagementDto>.Failure("Username already exists");

        var role = await _roleRepo.GetByIdAsync(dto.RoleId, ct);
        if (role is null)
            return Result<UserManagementDto>.Failure("Invalid role");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.IsVerified = true;
        user.Status = (byte)UserStatus.Active;
        user.CreatedAt = DateTime.UtcNow;

        await _userRepo.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(dto.FullName) || !string.IsNullOrWhiteSpace(dto.PhoneNumber) || !string.IsNullOrWhiteSpace(dto.Address))
        {
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address
            };
            await _profileRepo.AddAsync(profile, ct);
            await _uow.SaveChangesAsync(ct);
        }

        _logger.LogInformation("User {UserId} ({Username}) created by admin", user.UserId, user.Username);

        var result = await BuildUserDtoAsync(user, ct);
        return Result<UserManagementDto>.Success(result);
    }

    public async Task<Result<UserManagementDto>> UpdateAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<UserManagementDto>.Failure(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var user = await _userRepo.GetByIdAsync(dto.UserId, ct);
        if (user is null)
            return Result<UserManagementDto>.Failure("User not found");

        if (dto.Email is not null && dto.Email != user.Email)
        {
            var existsEmail = await _userRepo.AnyAsync(u => u.Email == dto.Email && u.UserId != dto.UserId, ct);
            if (existsEmail)
                return Result<UserManagementDto>.Failure("Email already exists");
            user.Email = dto.Email;
        }

        if (dto.Username is not null && dto.Username != user.Username)
        {
            var existsUsername = await _userRepo.AnyAsync(u => u.Username == dto.Username && u.UserId != dto.UserId, ct);
            if (existsUsername)
                return Result<UserManagementDto>.Failure("Username already exists");
            user.Username = dto.Username;
        }

        if (dto.RoleId.HasValue)
        {
            var role = await _roleRepo.GetByIdAsync(dto.RoleId.Value, ct);
            if (role is null)
                return Result<UserManagementDto>.Failure("Invalid role");
            user.RoleId = dto.RoleId.Value;
        }

        if (dto.Status.HasValue)
            user.Status = dto.Status.Value;

        if (dto.IsVerified.HasValue)
            user.IsVerified = dto.IsVerified.Value;

        _userRepo.Update(user);

        // Update profile
        if (dto.FullName is not null || dto.PhoneNumber is not null || dto.Address is not null)
        {
            var profiles = await _profileRepo.FindAsync(p => p.UserId == user.UserId, ct);
            var profile = profiles.FirstOrDefault();

            if (profile is null)
            {
                profile = new UserProfile
                {
                    UserId = user.UserId,
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
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated by admin", user.UserId);

        var result = await BuildUserDtoAsync(user, ct);
        return Result<UserManagementDto>.Success(result);
    }

    public async Task<Result> DeleteAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure("User not found");

        if (user.Status == (byte)UserStatus.Deleted)
            return Result.Failure("User is already deleted");

        // Soft delete: mark user as deleted instead of removing from database
        user.Status = (byte)UserStatus.Deleted;
        user.IsVerified = false;
        _userRepo.Update(user);

        // Remove the user profile to clear personal details (Privacy protection)
        var profiles = await _profileRepo.FindAsync(p => p.UserId == userId, ct);
        var profile = profiles.FirstOrDefault();
        if (profile is not null)
        {
            _profileRepo.Delete(profile);
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} ({Username}) soft-deleted by admin", userId, user.Username);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(int userId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return Result.Failure("Password must be at least 8 characters");

        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure("User not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Password reset for user {UserId} by admin", userId);

        return Result.Success();
    }

    #region Private Helpers

    /// <summary>
    /// Converts a UserStatus byte value to its display name using the enum.
    /// </summary>
    private static string GetStatusName(byte? status)
    {
        if (!status.HasValue) return "Unknown";
        return Enum.IsDefined(typeof(UserStatus), (int)status.Value)
            ? ((UserStatus)status.Value).ToString()
            : "Unknown";
    }

    /// <summary>
    /// Batch-friendly DTO builder using pre-loaded lookup dictionaries.
    /// Eliminates N+1 queries when building multiple DTOs.
    /// </summary>
    private static UserManagementDto BuildUserDto(
        User user,
        Dictionary<int, string> roleLookup,
        Dictionary<int, UserProfile> profileLookup,
        Dictionary<int, int> listingCounts,
        Dictionary<int, int> orderCounts)
    {
        profileLookup.TryGetValue(user.UserId, out var profile);

        return new UserManagementDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = roleLookup.GetValueOrDefault(user.RoleId, "Unknown"),
            Status = user.Status,
            StatusName = GetStatusName(user.Status),
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            FullName = profile?.FullName,
            PhoneNumber = profile?.PhoneNumber,
            Address = profile?.Address,
            AvatarUrl = profile?.AvatarUrl,
            TotalListings = listingCounts.GetValueOrDefault(user.UserId, 0),
            TotalOrders = orderCounts.GetValueOrDefault(user.UserId, 0)
        };
    }

    /// <summary>
    /// Single-user DTO builder for GetById/Create/Update (individual queries acceptable).
    /// </summary>
    private async Task<UserManagementDto> BuildUserDtoAsync(User user, CancellationToken ct)
    {
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var profiles = await _profileRepo.FindAsync(p => p.UserId == user.UserId, ct);
        var profile = profiles.FirstOrDefault();
        var listingCount = await _listingRepo.CountAsync(l => l.SellerId == user.UserId, ct);
        var orderCount = await _orderRepo.CountAsync(o => o.BuyerId == user.UserId, ct);

        return new UserManagementDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = role?.RoleName ?? "Unknown",
            Status = user.Status,
            StatusName = GetStatusName(user.Status),
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            FullName = profile?.FullName,
            PhoneNumber = profile?.PhoneNumber,
            Address = profile?.Address,
            AvatarUrl = profile?.AvatarUrl,
            TotalListings = listingCount,
            TotalOrders = orderCount
        };
    }

    #endregion
}