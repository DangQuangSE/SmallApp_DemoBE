using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Validates Google ID tokens using the Google.Apis.Auth library.
/// </summary>
public sealed class GoogleTokenValidatorService : IGoogleTokenValidator
{
    private readonly string _clientId;
    private readonly ILogger<GoogleTokenValidatorService> _logger;

    public GoogleTokenValidatorService(
        IConfiguration configuration,
        ILogger<GoogleTokenValidatorService> logger)
    {
        _clientId = configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is not configured.");
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (string.IsNullOrEmpty(payload.Email))
            {
                _logger.LogWarning("Google token validated but email is missing");
                return null;
            }

            return new GoogleUserInfo(
                Email: payload.Email,
                EmailVerified: payload.EmailVerified,
                Name: payload.Name ?? payload.Email,
                Picture: payload.Picture,
                GoogleId: payload.Subject,
                GivenName: payload.GivenName,
                FamilyName: payload.FamilyName,
                Locale: payload.Locale
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token received");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating Google token");
            return null;
        }
    }
}
