using FluentValidation;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.Symptoms)
            .NotEmpty().WithMessage("Symptoms are required.")
            .MaximumLength(1000).WithMessage("Symptoms must not exceed 1000 characters.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required.")
            .MaximumLength(1000).WithMessage("Diagnosis must not exceed 1000 characters.");

        RuleFor(x => x.Treatment)
            .NotEmpty().WithMessage("Treatment is required.")
            .MaximumLength(1000).WithMessage("Treatment must not exceed 1000 characters.");

        RuleFor(x => x.BloodPressure)
            .NotEmpty().WithMessage("Blood pressure is required.")
            .MaximumLength(20).WithMessage("Blood pressure must not exceed 20 characters.");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(30.0m, 45.0m)
            .WithMessage("Temperature must be between 30.0°C and 45.0°C.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");
    }
}
