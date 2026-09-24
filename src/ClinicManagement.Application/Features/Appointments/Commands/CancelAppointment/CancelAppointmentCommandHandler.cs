using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelAppointmentCommandHandler> _logger;

    public CancelAppointmentCommandHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CancelAppointmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<AppointmentDto>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
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
            throw new ValidationException("Appointment is already cancelled.");
        }

        var role = _currentUserService.Role;
        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (!currentPatientId.HasValue || appointment.PatientId != currentPatientId.Value)
            {
                throw new ForbiddenException("Patients can only cancel their own appointments.");
            }

            if (appointment.AppointmentDateTime <= DateTime.UtcNow)
            {
                throw new ValidationException("Patients can only cancel upcoming appointments.");
            }
        }
        else if (role != "Receptionist")
        {
            throw new ForbiddenException("You are not authorized to cancel appointments.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Appointment {AppointmentId} cancelled by user role {Role}",
            appointment.Id, role);

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

        return ApiResponse<AppointmentDto>.Succeeded(dto, "Appointment cancelled successfully.");
    }
}
