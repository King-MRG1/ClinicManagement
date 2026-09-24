using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Doctors;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityQueryHandler : IRequestHandler<GetDoctorAvailabilityQuery, ApiResponse<DoctorAvailabilityDto>>
{
    private readonly IClinicDbContext _context;

    public GetDoctorAvailabilityQueryHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DoctorAvailabilityDto>> Handle(GetDoctorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.DoctorId);
        }

        var startOfDay = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = request.Date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var existingAppointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == request.DoctorId
                        && a.Status != AppointmentStatus.Cancelled
                        && a.AppointmentDateTime >= startOfDay
                        && a.AppointmentDateTime <= endOfDay)
            .Select(a => new { a.AppointmentDateTime, a.EndDateTime })
            .ToListAsync(cancellationToken);

        var slots = new List<TimeSlotDto>();
        var workStart = request.Date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        var workEnd = request.Date.ToDateTime(new TimeOnly(17, 0), DateTimeKind.Utc);

        var currentSlotStart = workStart;
        var now = DateTime.UtcNow;

        while (currentSlotStart < workEnd)
        {
            var currentSlotEnd = currentSlotStart.AddMinutes(30);

            var isOverlapping = existingAppointments.Any(a =>
                a.AppointmentDateTime < currentSlotEnd && a.EndDateTime > currentSlotStart);

            var isPast = currentSlotStart < now;

            slots.Add(new TimeSlotDto
            {
                StartTime = currentSlotStart,
                EndTime = currentSlotEnd,
                IsAvailable = !isOverlapping && !isPast
            });

            currentSlotStart = currentSlotEnd;
        }

        var availability = new DoctorAvailabilityDto
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.User.FullName,
            Date = request.Date,
            Slots = slots
        };

        return ApiResponse<DoctorAvailabilityDto>.Succeeded(availability);
    }
}
