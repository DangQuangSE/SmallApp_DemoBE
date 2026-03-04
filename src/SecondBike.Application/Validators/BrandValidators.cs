using FluentValidation;
using SecondBike.Application.DTOs.Brands;

namespace SecondBike.Application.Validators;

public class CreateBrandValidator : AbstractValidator<CreateBrandDto>
{
    public CreateBrandValidator()
    {
        RuleFor(x => x.BrandName)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters");

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("Country must not exceed 50 characters")
            .When(x => x.Country is not null);
    }
}

public class UpdateBrandValidator : AbstractValidator<UpdateBrandDto>
{
    public UpdateBrandValidator()
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Valid brand ID is required");

        RuleFor(x => x.BrandName)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters");

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("Country must not exceed 50 characters")
            .When(x => x.Country is not null);
    }
}
