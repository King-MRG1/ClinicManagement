using ClinicManagement.Application.DTOs.MedicalRecords;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecords;

public record GetMedicalRecordsQuery(Guid? PatientId = null) : IRequest<List<MedicalRecordDto>>;
