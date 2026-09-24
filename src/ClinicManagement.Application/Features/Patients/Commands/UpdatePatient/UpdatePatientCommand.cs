using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand : IRequest<ApiResponse<PatientDto>>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string Address { get; init; } = string.Empty;
}
