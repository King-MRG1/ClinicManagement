using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId) : IRequest<ApiResponse<AppointmentDto>>;
