using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, ApiResponse<AppointmentDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentByIdQueryHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (appointment == null)
        {
            throw new NotFoundException("Appointment", request.Id);
        }

        var role = _currentUserService.Role;
        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (appointment.PatientId != currentPatientId)
            {
                throw new ForbiddenException("Patients can only view their own appointments.");
            }
        }
        else if (role == "Doctor")
        {
            var currentDoctorId = await _currentUserService.GetCurrentDoctorIdAsync(cancellationToken);
            if (appointment.DoctorId != currentDoctorId)
            {
                throw new ForbiddenException("Doctors can only view their own appointments.");
            }
        }
        else if (role != "Receptionist")
        {
            throw new ForbiddenException();
        }

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor.User.FullName,
            DoctorSpecialization = appointment.Doctor.Specialization,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.User.FullName,
            AppointmentDateTime = appointment.AppointmentDateTime,
            EndDateTime = appointment.EndDateTime,
            Status = appointment.Status,
            Reason = appointment.Reason,
            CreatedAt = appointment.CreatedAt
        };

        return ApiResponse<AppointmentDto>.Succeeded(dto);
    }
}
