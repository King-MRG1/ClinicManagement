using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<AppointmentDto>
{
    public Guid DoctorId { get; init; }
    public Guid PatientId { get; init; }
    public DateTime AppointmentDate { get; init; }
}
