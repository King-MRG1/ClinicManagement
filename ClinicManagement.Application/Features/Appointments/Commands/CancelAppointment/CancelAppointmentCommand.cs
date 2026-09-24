using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid Id) : IRequest<AppointmentDto>;
