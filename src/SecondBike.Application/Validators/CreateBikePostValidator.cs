using FluentValidation;
using SecondBike.Application.DTOs.Bikes;

namespace SecondBike.Application.Validators;

public class CreateBikePostValidator : AbstractValidator<CreateBikePostDto>
{
    private const int MaxTotalImages = 10;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public CreateBikePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1");

        RuleFor(x => x)
            .Must(x => x.Images.Count + x.ImageUrls.Count > 0)
            .WithMessage("At least one image is required")
            .Must(x => x.Images.Count + x.ImageUrls.Count <= MaxTotalImages)
            .WithMessage($"Maximum {MaxTotalImages} images allowed");

        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(f => f.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("Each image must be at most 5 MB");

            image.RuleFor(f => f.FileName)
                .Must(fileName => AllowedExtensions.Any(ext =>
                    fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Only .jpg, .jpeg, .png, .webp images are allowed");
        });

        RuleFor(x => x.Condition)
            .MaximumLength(50).WithMessage("Condition must be at most 50 characters")
            .When(x => x.Condition is not null);

        RuleFor(x => x.Address)
            .MaximumLength(255).WithMessage("Address must be at most 255 characters")
            .When(x => x.Address is not null);
    }
}
