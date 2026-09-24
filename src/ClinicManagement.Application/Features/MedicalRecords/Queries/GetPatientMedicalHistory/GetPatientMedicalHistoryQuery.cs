using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.MedicalRecords;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecords.Queries.GetPatientMedicalHistory;

public record GetPatientMedicalHistoryQuery(Guid PatientId) : IRequest<ApiResponse<List<MedicalRecordDto>>>;
