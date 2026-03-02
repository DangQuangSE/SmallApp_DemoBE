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

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must be at most 5000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(1990, DateTime.UtcNow.Year + 1)
            .WithMessage("Year must be valid");

        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Size).IsInEnum();
        RuleFor(x => x.Condition).IsInEnum();

        RuleFor(x => x.FrameMaterial).NotEmpty();
        RuleFor(x => x.Color).NotEmpty();

        RuleFor(x => x.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.District).NotEmpty();

        RuleFor(x => x.ImageUrls)
            .Must(x => x.Count > 0).WithMessage("At least one image is required")
            .Must(x => x.Count <= 10).WithMessage("Maximum 10 images allowed");
    }
}
