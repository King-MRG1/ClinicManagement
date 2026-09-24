using ClinicManagement.Application.DTOs.Appointments;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(Guid? DoctorId = null, Guid? PatientId = null) : IRequest<List<AppointmentDto>>;
