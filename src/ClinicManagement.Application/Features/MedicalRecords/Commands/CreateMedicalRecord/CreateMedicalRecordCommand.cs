using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.MedicalRecords;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public record CreateMedicalRecordCommand : IRequest<ApiResponse<MedicalRecordDto>>
{
    public Guid PatientId { get; init; }
    public Guid? AppointmentId { get; init; }
    public string Symptoms { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string Treatment { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string BloodPressure { get; init; } = string.Empty;
    public decimal Temperature { get; init; }
}
