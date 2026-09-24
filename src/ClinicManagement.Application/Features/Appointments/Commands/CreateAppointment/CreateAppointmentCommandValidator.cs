using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.AppointmentDateTime)
            .NotEmpty().WithMessage("Appointment date and time is required.")
            .Must(dt => dt > DateTime.UtcNow)
            .WithMessage("Appointment cannot be scheduled in the past.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
