using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery : IRequest<ApiResponse<List<AppointmentDto>>>
{
    public DateOnly? Date { get; init; }
    public Guid? DoctorId { get; init; }
    public Guid? PatientId { get; init; }
    public AppointmentStatus? Status { get; init; }
    public bool? UpcomingOnly { get; init; }
}
