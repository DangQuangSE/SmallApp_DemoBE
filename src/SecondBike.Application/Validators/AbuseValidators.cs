using FluentValidation;
using SecondBike.Application.DTOs.Abuse;

namespace SecondBike.Application.Validators;

public class CreateAbuseRequestValidator : AbstractValidator<CreateAbuseRequestDto>
{
    public CreateAbuseRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(1000).WithMessage("Reason must be at most 1000 characters");

        RuleFor(x => x)
            .Must(x => x.TargetListingId.HasValue || x.TargetUserId.HasValue)
            .WithMessage("Must specify either a listing or a user to report");

        RuleFor(x => x.TargetListingId)
            .GreaterThan(0).When(x => x.TargetListingId.HasValue)
            .WithMessage("Invalid listing ID");

        RuleFor(x => x.TargetUserId)
            .GreaterThan(0).When(x => x.TargetUserId.HasValue)
            .WithMessage("Invalid user ID");
    }
}

public class ResolveAbuseRequestValidator : AbstractValidator<ResolveAbuseRequestDto>
{
    public ResolveAbuseRequestValidator()
    {
        RuleFor(x => x.RequestAbuseId)
            .GreaterThan(0).WithMessage("Invalid request ID");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required")
            .MaximumLength(2000).WithMessage("Resolution must be at most 2000 characters");

        RuleFor(x => x.Status)
            .InclusiveBetween((byte)1, (byte)3)
            .WithMessage("Status must be between 1 (Pending) and 3 (Rejected)");
    }
}
