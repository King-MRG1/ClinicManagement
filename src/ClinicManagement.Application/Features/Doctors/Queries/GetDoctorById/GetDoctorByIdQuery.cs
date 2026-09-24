using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Doctors;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;

public record GetDoctorByIdQuery(Guid Id) : IRequest<ApiResponse<DoctorDto>>;
