using FluentValidation;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Validators;

public class CreateBikePostValidator : AbstractValidator<CreateBikePostDto>
{
    public CreateBikePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.ImageUrls)
            .Must(x => x.Count > 0).WithMessage("At least one image is required")
            .Must(x => x.Count <= 10).WithMessage("Maximum 10 images allowed");
    }
}
