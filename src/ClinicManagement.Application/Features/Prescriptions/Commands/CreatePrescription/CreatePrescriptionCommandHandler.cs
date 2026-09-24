using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Prescriptions;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, ApiResponse<PrescriptionDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreatePrescriptionCommandHandler> _logger;

    public CreatePrescriptionCommandHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreatePrescriptionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PrescriptionDto>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        if (role != "Doctor")
        {
            throw new ForbiddenException("Only Doctors can create prescriptions.");
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
            throw new ForbiddenException("Doctor cannot access or prescribe for unrelated patients.");
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

        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            AppointmentId = request.AppointmentId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescription.Id,
                MedicineName = item.MedicineName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                Duration = item.Duration,
                Instructions = item.Instructions
            });
        }

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Prescription {PrescriptionId} with {ItemCount} items created by Doctor {DoctorId} for Patient {PatientId}",
            prescription.Id, prescription.PrescriptionItems.Count, doctor.Id, patient.Id);

        var dto = new PrescriptionDto
        {
            Id = prescription.Id,
            PatientId = patient.Id,
            PatientName = patient.User.FullName,
            DoctorId = doctor.Id,
            DoctorName = doctor.User.FullName,
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

        return ApiResponse<PrescriptionDto>.Succeeded(dto, "Prescription created successfully.");
    }
}
