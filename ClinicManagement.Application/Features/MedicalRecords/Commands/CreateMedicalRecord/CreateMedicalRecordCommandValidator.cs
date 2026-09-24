using FluentValidation;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator()
    {
        RuleFor(m => m.PatientId).NotEmpty().WithMessage("PatientId is required.");
        RuleFor(m => m.Diagnosis).NotEmpty().WithMessage("Diagnosis is required.");
        RuleFor(m => m.Treatment).NotEmpty().WithMessage("Treatment is required.");
    }
}
