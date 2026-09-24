using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ILogger<RescheduleAppointmentCommandHandler> _logger;

    public RescheduleAppointmentCommandHandler(
        IClinicDbContext context,
        ILogger<RescheduleAppointmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment == null)
        {
            throw new NotFoundException("Appointment", request.AppointmentId);
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            throw new ValidationException("Cannot reschedule a cancelled appointment.");
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            throw new ValidationException("Cannot reschedule a completed appointment.");
        }

        var newStart = request.NewAppointmentDateTime;
        if (newStart < DateTime.UtcNow)
        {
            throw new ValidationException("Cannot reschedule appointment to the past.");
        }

        var newEnd = newStart.AddMinutes(30);

        // Check Doctor overlap (excluding this appointment)
        var doctorHasOverlap = await _context.Appointments
            .AnyAsync(a => a.Id != appointment.Id
                           && a.DoctorId == appointment.DoctorId
                           && a.Status != AppointmentStatus.Cancelled
                           && a.AppointmentDateTime < newEnd
                           && a.EndDateTime > newStart, cancellationToken);

        if (doctorHasOverlap)
        {
            throw new ConflictException("Doctor already has an appointment at this time.");
        }

        // Check Patient overlap (excluding this appointment)
        var patientHasOverlap = await _context.Appointments
            .AnyAsync(a => a.Id != appointment.Id
                           && a.PatientId == appointment.PatientId
                           && a.Status != AppointmentStatus.Cancelled
                           && a.AppointmentDateTime < newEnd
                           && a.EndDateTime > newStart, cancellationToken);

        if (patientHasOverlap)
        {
            throw new ConflictException("Patient already has an appointment at this time.");
        }

        appointment.AppointmentDateTime = newStart;
        appointment.EndDateTime = newEnd;
        appointment.Status = AppointmentStatus.Scheduled;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Appointment {AppointmentId} rescheduled to {NewTime}",
            appointment.Id, newStart);

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor.User.FullName,
            DoctorSpecialization = appointment.Doctor.Specialization,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.User.FullName,
            AppointmentDateTime = appointment.AppointmentDateTime,
            EndDateTime = appointment.EndDateTime,
            Status = appointment.Status,
            Reason = appointment.Reason,
            CreatedAt = appointment.CreatedAt
        };

        return ApiResponse<AppointmentDto>.Succeeded(dto, "Appointment rescheduled successfully.");
    }
}
