using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Prescriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Prescriptions.Queries.GetPrescriptionById;

public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery, ApiResponse<PrescriptionDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPrescriptionByIdQueryHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PrescriptionDto>> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var prescription = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Patient).ThenInclude(pt => pt.User)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.PrescriptionItems)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (prescription == null)
        {
            throw new NotFoundException("Prescription", request.Id);
        }

        var role = _currentUserService.Role;
        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (prescription.PatientId != currentPatientId)
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
                .AnyAsync(a => a.DoctorId == currentDoctorId.Value && a.PatientId == prescription.PatientId, cancellationToken);

            if (!hasAppointment && prescription.DoctorId != currentDoctorId.Value)
            {
                throw new ForbiddenException("Doctors can only view prescriptions of patients who have appointments with them.");
            }
        }
        else
        {
            throw new ForbiddenException("You are not authorized to view prescriptions.");
        }

        var dto = new PrescriptionDto
        {
            Id = prescription.Id,
            PatientId = prescription.PatientId,
            PatientName = prescription.Patient.User.FullName,
            DoctorId = prescription.DoctorId,
            DoctorName = prescription.Doctor.User.FullName,
            AppointmentId = prescription.AppointmentId,
            Notes = prescription.Notes,
            CreatedAt = prescription.CreatedAt,
            Items = prescription.PrescriptionItems.Select(i => new PrescriptionItemDto
            {
                Id = i.Id,
                MedicineName = i.MedicineName,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Instructions = i.Instructions
            }).ToList()
        };

        return ApiResponse<PrescriptionDto>.Succeeded(dto);
    }
}
