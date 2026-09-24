using FluentValidation;

namespace ClinicManagement.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionItemCommandValidator : AbstractValidator<CreatePrescriptionItemCommand>
{
    public CreatePrescriptionItemCommandValidator()
    {
        RuleFor(x => x.MedicineName)
            .NotEmpty().WithMessage("Medicine name is required.")
            .MaximumLength(150).WithMessage("Medicine name must not exceed 150 characters.");

        RuleFor(x => x.Dosage)
            .NotEmpty().WithMessage("Dosage is required.")
            .MaximumLength(100).WithMessage("Dosage must not exceed 100 characters.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required.")
            .MaximumLength(100).WithMessage("Frequency must not exceed 100 characters.");

        RuleFor(x => x.Duration)
            .NotEmpty().WithMessage("Duration is required.")
            .MaximumLength(100).WithMessage("Duration must not exceed 100 characters.");

        RuleFor(x => x.Instructions)
            .NotEmpty().WithMessage("Instructions are required.")
            .MaximumLength(500).WithMessage("Instructions must not exceed 500 characters.");
    }
}

public class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one prescription item is required.");

        RuleForEach(x => x.Items).SetValidator(new CreatePrescriptionItemCommandValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}
