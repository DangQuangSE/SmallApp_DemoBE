namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Validates Google OAuth ID tokens and extracts user information.
/// Implementation resides in the Infrastructure layer (DIP).
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validates a Google ID token and returns the user's information.
    /// </summary>
    /// <param name="idToken">The Google ID token from the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User info if valid; null if the token is invalid or expired.</returns>
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default);
}

/// <summary>
/// Represents the user information extracted from a validated Google token.
/// Enhanced with all essential Google JWT payload fields.
/// </summary>
public record GoogleUserInfo(
    string Email,
    bool EmailVerified,
    string Name,
    string? Picture,
    string GoogleId,
    string? GivenName = null,
    string? FamilyName = null,
    string? Locale = null
);
