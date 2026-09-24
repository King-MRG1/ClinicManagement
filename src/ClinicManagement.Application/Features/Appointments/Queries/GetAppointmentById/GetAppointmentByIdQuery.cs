using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<ApiResponse<AppointmentDto>>;
