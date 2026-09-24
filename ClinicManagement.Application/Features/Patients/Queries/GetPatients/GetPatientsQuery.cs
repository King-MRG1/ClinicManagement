using ClinicManagement.Application.DTOs.Patients;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery(string? SearchTerm = null) : IRequest<List<PatientDto>>;
