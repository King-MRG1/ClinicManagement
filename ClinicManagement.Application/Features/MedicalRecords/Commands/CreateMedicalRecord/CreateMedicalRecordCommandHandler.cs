using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.MedicalRecords;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, MedicalRecordDto>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateMedicalRecordCommandHandler(IClinicDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<MedicalRecordDto> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Doctor")
        {
            throw new ForbiddenException("Only doctors can create medical records.");
        }

        var doctorId = _currentUserService.DoctorId;
        if (!doctorId.HasValue)
        {
            throw new UnauthorizedException("Doctor profile not found.");
        }

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId.Value, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", doctorId.Value);
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.PatientId);
        }

        // Verify the doctor has at least one appointment with that patient, otherwise 403
        var hasAppointment = await _context.Appointments
            .AnyAsync(a => a.DoctorId == doctor.Id && a.PatientId == patient.Id, cancellationToken);

        if (!hasAppointment)
        {
            throw new ForbiddenException("Doctor cannot create records for patients who have no appointment with them.");
        }

        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            VisitDate = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        return new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = patient.Id,
            PatientName = patient.FullName,
            DoctorId = doctor.Id,
            DoctorName = doctor.User.FullName,
            Diagnosis = record.Diagnosis,
            Treatment = record.Treatment,
            Notes = record.Notes,
            VisitDate = record.VisitDate
        };
    }
}
