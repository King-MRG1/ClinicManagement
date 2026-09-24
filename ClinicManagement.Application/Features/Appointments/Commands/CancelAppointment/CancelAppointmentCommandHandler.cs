using ClinicManagement.Application.Abstractions;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Application.Interfaces;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, AppointmentDto>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelAppointmentCommandHandler(IClinicDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AppointmentDto> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (appointment == null)
        {
            throw new KeyNotFoundException($"Appointment with id '{request.Id}' was not found.");
        }

        if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot cancel an appointment that is already {appointment.Status.ToString().ToLower()}.");
        }

        // If patient, ensure they only cancel their own appointment
        if (_currentUserService.Role == Roles.Patient)
        {
            if (_currentUserService.PatientId != appointment.PatientId)
            {
                throw new UnauthorizedAccessException("Forbidden: You can only cancel your own appointments.");
            }
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor.User.FullName,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.FullName,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status
        };
    }
}
