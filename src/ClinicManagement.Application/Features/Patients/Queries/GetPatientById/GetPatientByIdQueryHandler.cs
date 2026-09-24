using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, ApiResponse<PatientDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientByIdQueryHandler(
        IClinicDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.Id);
        }

        var role = _currentUserService.Role;
        if (role == "Patient")
        {
            var currentPatientId = await _currentUserService.GetCurrentPatientIdAsync(cancellationToken);
            if (currentPatientId != patient.Id)
            {
                throw new ForbiddenException("Patients can only view their own profile.");
            }
        }
        else if (role == "Doctor")
        {
            var doctorId = await _currentUserService.GetCurrentDoctorIdAsync(cancellationToken);
            var hasAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId && a.PatientId == patient.Id, cancellationToken);

            if (!hasAppointment)
            {
                throw new ForbiddenException("Doctors can only access patients who have consultations with them.");
            }
        }

        var dto = new PatientDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            FullName = patient.User.FullName,
            Email = patient.User.Email ?? string.Empty,
            PhoneNumber = patient.User.PhoneNumber ?? string.Empty,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Address = patient.Address
        };

        return ApiResponse<PatientDto>.Succeeded(dto);
    }
}
