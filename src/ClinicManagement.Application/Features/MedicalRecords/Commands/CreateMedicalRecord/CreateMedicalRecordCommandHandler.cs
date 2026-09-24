using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.MedicalRecords;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, ApiResponse<MedicalRecordDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateMedicalRecordCommandHandler> _logger;

    public CreateMedicalRecordCommandHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateMedicalRecordCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<MedicalRecordDto>> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        if (role != "Doctor")
        {
            throw new ForbiddenException("Only Doctors can create medical records.");
        }

        var doctorId = await _currentUserService.GetCurrentDoctorIdAsync(cancellationToken);
        if (!doctorId.HasValue)
        {
            throw new UnauthorizedException("Authenticated doctor profile was not found.");
        }

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId.Value, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", doctorId.Value);
        }

        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.PatientId);
        }

        // Verify doctor has consultation / appointment with this patient
        var hasAppointment = await _context.Appointments
            .AnyAsync(a => a.DoctorId == doctor.Id && a.PatientId == patient.Id, cancellationToken);

        if (!hasAppointment)
        {
            throw new ForbiddenException("Doctor cannot access or create records for unrelated patients.");
        }

        if (request.AppointmentId.HasValue)
        {
            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId.Value
                                          && a.DoctorId == doctor.Id
                                          && a.PatientId == patient.Id, cancellationToken);
            if (appt == null)
            {
                throw new NotFoundException("Appointment", request.AppointmentId.Value);
            }
        }

        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            AppointmentId = request.AppointmentId,
            Symptoms = request.Symptoms,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            BloodPressure = request.BloodPressure,
            Temperature = request.Temperature,
            CreatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Medical record {RecordId} created by Doctor {DoctorId} for Patient {PatientId}",
            record.Id, doctor.Id, patient.Id);

        var dto = new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = patient.Id,
            PatientName = patient.User.FullName,
            DoctorId = doctor.Id,
            DoctorName = doctor.User.FullName,
            AppointmentId = record.AppointmentId,
            Symptoms = record.Symptoms,
            Diagnosis = record.Diagnosis,
            Treatment = record.Treatment,
            Notes = record.Notes,
            BloodPressure = record.BloodPressure,
            Temperature = record.Temperature,
            CreatedAt = record.CreatedAt
        };

        return ApiResponse<MedicalRecordDto>.Succeeded(dto, "Medical record created successfully.");
    }
}
