using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("Appointment ID is required.");

        RuleFor(x => x.NewAppointmentDateTime)
            .NotEmpty().WithMessage("New appointment date and time is required.")
            .Must(dt => dt > DateTime.UtcNow)
            .WithMessage("Appointment cannot be rescheduled to the past.");
    }
}
