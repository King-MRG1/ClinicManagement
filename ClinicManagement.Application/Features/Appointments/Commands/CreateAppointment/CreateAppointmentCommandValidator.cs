using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("DoctorId is required.");
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("PatientId is required.");
        RuleFor(x => x.AppointmentDate).NotEmpty().WithMessage("Appointment date is required.");
    }
}
