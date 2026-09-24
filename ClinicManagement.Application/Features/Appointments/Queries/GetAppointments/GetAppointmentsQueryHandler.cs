using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentsQueryHandler(IClinicDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .AsQueryable();

        // If patient, restrict to their own appointments
        if (_currentUserService.Role == "Patient")
        {
            var patientId = _currentUserService.PatientId;
            query = query.Where(a => a.PatientId == patientId);
        }
        else
        {
            if (request.PatientId.HasValue)
            {
                query = query.Where(a => a.PatientId == request.PatientId.Value);
            }
            if (request.DoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == request.DoctorId.Value);
            }
        }

        return await query
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                PatientId = a.PatientId,
                PatientName = a.Patient.FullName,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status
            })
            .ToListAsync(cancellationToken);
    }
}
