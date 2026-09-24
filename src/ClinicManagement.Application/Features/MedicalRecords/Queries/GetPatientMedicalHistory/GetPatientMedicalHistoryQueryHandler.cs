using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.MedicalRecords;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Queries.GetPatientMedicalHistory;

public class GetPatientMedicalHistoryQueryHandler : IRequestHandler<GetPatientMedicalHistoryQuery, ApiResponse<List<MedicalRecordDto>>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientMedicalHistoryQueryHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<List<MedicalRecordDto>>> Handle(GetPatientMedicalHistoryQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;

        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (!currentPatientId.HasValue || currentPatientId.Value != request.PatientId)
            {
                throw new ForbiddenException("Patients can only view their own medical records.");
            }
        }
        else if (role == "Doctor")
        {
            var currentDoctorId = await _currentUserService.GetCurrentDoctorIdAsync(cancellationToken);
            if (!currentDoctorId.HasValue)
            {
                throw new UnauthorizedException();
            }

            var hasAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == currentDoctorId.Value && a.PatientId == request.PatientId, cancellationToken);

            if (!hasAppointment)
            {
                throw new ForbiddenException("Doctors can only access medical records of patients who have appointments with them.");
            }
        }
        else
        {
            throw new ForbiddenException("You are not authorized to view medical records.");
        }

        var records = await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .Where(m => m.PatientId == request.PatientId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                PatientName = m.Patient.User.FullName,
                DoctorId = m.DoctorId,
                DoctorName = m.Doctor.User.FullName,
                AppointmentId = m.AppointmentId,
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Treatment = m.Treatment,
                Notes = m.Notes,
                BloodPressure = m.BloodPressure,
                Temperature = m.Temperature,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<MedicalRecordDto>>.Succeeded(records);
    }
}
