using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand : IRequest<ApiResponse<AppointmentDto>>
{
    public Guid AppointmentId { get; init; }
    public DateTime NewAppointmentDateTime { get; init; }
}
