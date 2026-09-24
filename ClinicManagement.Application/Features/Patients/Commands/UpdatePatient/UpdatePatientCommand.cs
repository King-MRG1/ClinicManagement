using ClinicManagement.Application.DTOs.Patients;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand : IRequest<PatientDto>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
}
