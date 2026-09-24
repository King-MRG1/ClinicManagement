using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<ApiResponse<PatientDto>>;
