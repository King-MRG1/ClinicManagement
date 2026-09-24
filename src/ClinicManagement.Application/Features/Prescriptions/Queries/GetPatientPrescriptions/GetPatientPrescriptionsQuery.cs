using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Prescriptions;
using MediatR;

namespace ClinicManagement.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;

public record GetPatientPrescriptionsQuery(Guid PatientId) : IRequest<ApiResponse<List<PrescriptionDto>>>;
