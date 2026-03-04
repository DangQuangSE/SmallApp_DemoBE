using FluentValidation;
using SecondBike.Application.DTOs.Inspections;

namespace SecondBike.Application.Validators;

public class CreateInspectionRequestValidator : AbstractValidator<CreateInspectionRequestDto>
{
    public CreateInspectionRequestValidator()
    {
        RuleFor(x => x.ListingId)
            .GreaterThan(0).WithMessage("Invalid listing ID");

        RuleFor(x => x.Note)
            .MaximumLength(500).When(x => x.Note is not null)
            .WithMessage("Note must be at most 500 characters");
    }
}

public class UploadInspectionReportValidator : AbstractValidator<UploadInspectionReportDto>
{
    public UploadInspectionReportValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0).WithMessage("Invalid request ID");

        RuleFor(x => x.FinalVerdict)
            .Must(v => v is null or 1 or 2 or 3)
            .WithMessage("FinalVerdict must be 1 (Pass), 2 (Fail), or 3 (Conditional)");

        RuleFor(x => x.FrameCheck)
            .MaximumLength(500).When(x => x.FrameCheck is not null);

        RuleFor(x => x.BrakeCheck)
            .MaximumLength(500).When(x => x.BrakeCheck is not null);

        RuleFor(x => x.TransmissionCheck)
            .MaximumLength(500).When(x => x.TransmissionCheck is not null);

        RuleFor(x => x.InspectorNote)
            .MaximumLength(2000).When(x => x.InspectorNote is not null);
    }
}
