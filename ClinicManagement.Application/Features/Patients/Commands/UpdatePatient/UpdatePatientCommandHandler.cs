using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, PatientDto>
{
    private readonly IClinicDbContext _context;

    public UpdatePatientCommandHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<PatientDto> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.Id);
        }

        patient.FullName = request.FullName;
        patient.PhoneNumber = request.PhoneNumber;
        patient.DateOfBirth = request.DateOfBirth;

        if (patient.User != null)
        {
            patient.User.FullName = request.FullName;
            patient.User.PhoneNumber = request.PhoneNumber;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PatientDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            FullName = patient.FullName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth
        };
    }
}
