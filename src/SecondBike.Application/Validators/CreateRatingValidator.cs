using FluentValidation;
using SecondBike.Application.DTOs.Ratings;

namespace SecondBike.Application.Validators;

public class CreateRatingValidator : AbstractValidator<CreateRatingDto>
{
    public CreateRatingValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Stars).InclusiveBetween(1, 5).WithMessage("Stars must be between 1 and 5");
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.CommunicationRating).InclusiveBetween(1, 5).When(x => x.CommunicationRating.HasValue);
        RuleFor(x => x.AccuracyRating).InclusiveBetween(1, 5).When(x => x.AccuracyRating.HasValue);
        RuleFor(x => x.PackagingRating).InclusiveBetween(1, 5).When(x => x.PackagingRating.HasValue);
        RuleFor(x => x.SpeedRating).InclusiveBetween(1, 5).When(x => x.SpeedRating.HasValue);
    }
}
