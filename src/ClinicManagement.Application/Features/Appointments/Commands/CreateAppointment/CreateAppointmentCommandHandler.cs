using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        Guid effectivePatientId;
        var role = _currentUserService.Role;

        if (role == "Patient")
        {
            var patientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (!patientId.HasValue)
            {
                throw new UnauthorizedException("Authenticated patient profile was not found.");
            }
            effectivePatientId = patientId.Value;
        }
        else if (role == "Receptionist")
        {
            if (!request.PatientId.HasValue || request.PatientId.Value == Guid.Empty)
            {
                throw new ValidationException("PatientId is required when creating an appointment as receptionist.");
            }
            effectivePatientId = request.PatientId.Value;
        }
        else
        {
            throw new ForbiddenException("Only Patients and Receptionists can book appointments.");
        }

        var start = request.AppointmentDateTime;
        if (start < DateTime.UtcNow)
        {
            throw new ValidationException("Cannot create appointments in the past.");
        }

        var end = start.AddMinutes(30);

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.DoctorId);
        }

        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == effectivePatientId, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", effectivePatientId);
        }

        var doctorOverlaps = await _context.Appointments
            .AnyAsync(a => a.DoctorId == request.DoctorId
                           && a.Status != AppointmentStatus.Cancelled
                           && a.AppointmentDateTime < end
                           && a.EndDateTime > start, cancellationToken);

        if (doctorOverlaps)
        {
            throw new ConflictException("Doctor already has an appointment at this time.");
        }

        var patientOverlaps = await _context.Appointments
            .AnyAsync(a => a.PatientId == effectivePatientId
                           && a.Status != AppointmentStatus.Cancelled
                           && a.AppointmentDateTime < end
                           && a.EndDateTime > start, cancellationToken);

        if (patientOverlaps)
        {
            throw new ConflictException("Patient already has an appointment at this time.");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            AppointmentDateTime = start,
            EndDateTime = end,
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Appointment {AppointmentId} created successfully for Doctor {DoctorId} and Patient {PatientId} at {Time}",
            appointment.Id, doctor.Id, patient.Id, start);

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = doctor.Id,
            DoctorName = doctor.User.FullName,
            DoctorSpecialization = doctor.Specialization,
            PatientId = patient.Id,
            PatientName = patient.User.FullName,
            AppointmentDateTime = appointment.AppointmentDateTime,
            EndDateTime = appointment.EndDateTime,
            Status = appointment.Status,
            Reason = appointment.Reason,
            CreatedAt = appointment.CreatedAt
        };

        return ApiResponse<AppointmentDto>.Succeeded(dto, "Appointment scheduled successfully.");
    }
}
