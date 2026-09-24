using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty();
        RuleFor(p => p.FullName).NotEmpty().WithMessage("Patient name is required.");
        RuleFor(p => p.DateOfBirth).NotEmpty();
    }
}
