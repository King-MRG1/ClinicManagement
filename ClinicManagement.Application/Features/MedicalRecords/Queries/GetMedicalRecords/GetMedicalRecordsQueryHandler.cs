using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.MedicalRecords;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecords;

public class GetMedicalRecordsQueryHandler : IRequestHandler<GetMedicalRecordsQuery, List<MedicalRecordDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMedicalRecordsQueryHandler(IClinicDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<MedicalRecordDto>> Handle(GetMedicalRecordsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        var query = _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .Include(m => m.Patient)
            .AsQueryable();

        if (role == "Patient")
        {
            var patientId = _currentUserService.PatientId;
            query = query.Where(m => m.PatientId == patientId);
        }
        else if (role == "Doctor")
        {
            var doctorId = _currentUserService.DoctorId;
            var patientIdsWithDoctor = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .ToListAsync(cancellationToken);

            query = query.Where(m => patientIdsWithDoctor.Contains(m.PatientId));

            if (request.PatientId.HasValue)
            {
                query = query.Where(m => m.PatientId == request.PatientId.Value);
            }
        }

        return await query
            .OrderByDescending(m => m.VisitDate)
            .Select(m => new MedicalRecordDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                PatientName = m.Patient.FullName,
                DoctorId = m.DoctorId,
                DoctorName = m.Doctor.User.FullName,
                Diagnosis = m.Diagnosis,
                Treatment = m.Treatment,
                Notes = m.Notes,
                VisitDate = m.VisitDate
            })
            .ToListAsync(cancellationToken);
    }
}
