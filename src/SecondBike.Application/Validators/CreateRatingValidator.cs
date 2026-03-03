using FluentValidation;
using SecondBike.Application.DTOs.Ratings;

namespace SecondBike.Application.Validators;

public class CreateRatingValidator : AbstractValidator<CreateRatingDto>
{
    public CreateRatingValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5");
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
