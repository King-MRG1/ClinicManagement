using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Prescriptions;
using MediatR;

namespace ClinicManagement.Application.Features.Prescriptions.Commands.CreatePrescription;

public record CreatePrescriptionItemCommand
{
    public string MedicineName { get; init; } = string.Empty;
    public string Dosage { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
}

public record CreatePrescriptionCommand : IRequest<ApiResponse<PrescriptionDto>>
{
    public Guid PatientId { get; init; }
    public Guid? AppointmentId { get; init; }
    public string? Notes { get; init; }
    public List<CreatePrescriptionItemCommand> Items { get; init; } = new();
}
