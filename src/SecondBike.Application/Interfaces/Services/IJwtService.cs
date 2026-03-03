using SecondBike.Domain.Entities;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for generating JWT tokens.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user, string roleName);
}
