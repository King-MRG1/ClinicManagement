using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Prescriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;

public class GetPatientPrescriptionsQueryHandler : IRequestHandler<GetPatientPrescriptionsQuery, ApiResponse<List<PrescriptionDto>>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientPrescriptionsQueryHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<List<PrescriptionDto>>> Handle(GetPatientPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;

        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (!currentPatientId.HasValue || currentPatientId.Value != request.PatientId)
            {
                throw new ForbiddenException("Patients can only view their own prescriptions.");
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
                throw new ForbiddenException("Doctors can only access prescriptions of patients who have appointments with them.");
            }
        }
        else
        {
            throw new ForbiddenException("You are not authorized to view prescriptions.");
        }

        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Patient).ThenInclude(pt => pt.User)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.PrescriptionItems)
            .Where(p => p.PatientId == request.PatientId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PrescriptionDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                PatientName = p.Patient.User.FullName,
                DoctorId = p.DoctorId,
                DoctorName = p.Doctor.User.FullName,
                AppointmentId = p.AppointmentId,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt,
                Items = p.PrescriptionItems.Select(i => new PrescriptionItemDto
                {
                    Id = i.Id,
                    MedicineName = i.MedicineName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<PrescriptionDto>>.Succeeded(prescriptions);
    }
}
