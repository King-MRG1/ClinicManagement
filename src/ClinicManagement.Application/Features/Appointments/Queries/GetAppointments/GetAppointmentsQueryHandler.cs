using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, ApiResponse<List<AppointmentDto>>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentsQueryHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<List<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsQueryable();

        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (!currentPatientId.HasValue)
            {
                throw new UnauthorizedException("Authenticated patient profile not found.");
            }
            query = query.Where(a => a.PatientId == currentPatientId.Value);
        }
        else if (role == "Doctor")
        {
            var currentDoctorId = await _currentUserService.GetCurrentDoctorIdAsync(cancellationToken);
            if (!currentDoctorId.HasValue)
            {
                throw new UnauthorizedException("Authenticated doctor profile not found.");
            }
            query = query.Where(a => a.DoctorId == currentDoctorId.Value);
        }
        else if (role == "Receptionist")
        {
            if (request.DoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == request.DoctorId.Value);
            }
            if (request.PatientId.HasValue)
            {
                query = query.Where(a => a.PatientId == request.PatientId.Value);
            }
        }
        else
        {
            throw new ForbiddenException();
        }

        if (request.Date.HasValue)
        {
            var startOfDay = request.Date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endOfDay = request.Date.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(a => a.AppointmentDateTime >= startOfDay && a.AppointmentDateTime <= endOfDay);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (request.UpcomingOnly == true)
        {
            query = query.Where(a => a.AppointmentDateTime > DateTime.UtcNow);
        }

        var appointments = await query
            .OrderBy(a => a.AppointmentDateTime)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                DoctorSpecialization = a.Doctor.Specialization,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                AppointmentDateTime = a.AppointmentDateTime,
                EndDateTime = a.EndDateTime,
                Status = a.Status,
                Reason = a.Reason,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<AppointmentDto>>.Succeeded(appointments);
    }
}
