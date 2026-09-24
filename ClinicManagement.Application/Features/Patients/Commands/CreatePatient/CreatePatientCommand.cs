using ClinicManagement.Application.DTOs.Patients;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand : IRequest<PatientDto>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
}
