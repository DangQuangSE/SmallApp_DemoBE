using FluentValidation;
using SecondBike.Application.DTOs.Users;

namespace SecondBike.Application.Validators;

/// <summary>
/// Validator for Google OAuth login requests.
/// Ensures the ID token is properly formatted and not empty.
/// </summary>
public class GoogleLoginValidator : AbstractValidator<GoogleLoginDto>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token is required")
            .MinimumLength(100).WithMessage("Invalid Google ID token format")
            .Must(BeValidJwtFormat).WithMessage("ID token must be in valid JWT format (xxx.yyy.zzz)");
    }

    /// <summary>
    /// Validates that the token has the basic JWT structure (3 parts separated by dots).
    /// </summary>
    private static bool BeValidJwtFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        return parts.Length == 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }
}
