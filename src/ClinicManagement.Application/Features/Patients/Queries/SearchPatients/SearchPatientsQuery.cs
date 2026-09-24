using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.SearchPatients;

public record SearchPatientsQuery(string? SearchTerm = null) : IRequest<ApiResponse<List<PatientDto>>>;
