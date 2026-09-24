using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand : IRequest<ApiResponse<PatientDto>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Password { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string Address { get; init; } = string.Empty;
}
