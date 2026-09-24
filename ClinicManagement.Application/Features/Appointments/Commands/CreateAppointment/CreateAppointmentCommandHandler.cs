using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IClinicDbContext _context;

    public CreateAppointmentCommandHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.DoctorId);
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.PatientId);
        }

        // Business rule: A doctor cannot have two appointments at the exact same date and time.
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.AppointmentDate == request.AppointmentDate &&
            a.Status != AppointmentStatus.Cancelled,
            cancellationToken);

        if (hasConflict)
        {
            throw new ConflictException("Doctor already has an appointment at this exact date and time.");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            AppointmentDate = request.AppointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = doctor.Id,
            DoctorName = doctor.User.FullName,
            PatientId = patient.Id,
            PatientName = patient.FullName,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status
        };
    }
}
