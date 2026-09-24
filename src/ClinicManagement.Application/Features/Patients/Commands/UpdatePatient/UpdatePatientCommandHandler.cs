using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, ApiResponse<PatientDto>>
{
    private readonly IClinicDbContext _context;
    private readonly ILogger<UpdatePatientCommandHandler> _logger;

    public UpdatePatientCommandHandler(
        IClinicDbContext context,
        ILogger<UpdatePatientCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.Id);
        }

        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.Address = request.Address;
        patient.User.FullName = request.FullName;
        patient.User.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Patient details updated for Patient ID {PatientId}", patient.Id);

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

        return ApiResponse<PatientDto>.Succeeded(dto, "Patient updated successfully.");
    }
}
