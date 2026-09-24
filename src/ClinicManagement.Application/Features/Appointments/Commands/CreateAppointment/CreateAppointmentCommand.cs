using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<ApiResponse<AppointmentDto>>
{
    public Guid DoctorId { get; init; }
    public Guid? PatientId { get; init; }
    public DateTime AppointmentDateTime { get; init; }
    public string? Reason { get; init; }
}
