using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(p => p.FullName).NotEmpty().WithMessage("Patient name is required.");
        RuleFor(p => p.Email).NotEmpty().EmailAddress();
        RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
        RuleFor(p => p.DateOfBirth).NotEmpty();
    }
}
