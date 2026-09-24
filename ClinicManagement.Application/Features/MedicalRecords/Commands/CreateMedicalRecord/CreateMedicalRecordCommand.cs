using ClinicManagement.Application.DTOs.MedicalRecords;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public record CreateMedicalRecordCommand : IRequest<MedicalRecordDto>
{
    public Guid PatientId { get; init; }
    public string Diagnosis { get; init; } = string.Empty;
    public string Treatment { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
