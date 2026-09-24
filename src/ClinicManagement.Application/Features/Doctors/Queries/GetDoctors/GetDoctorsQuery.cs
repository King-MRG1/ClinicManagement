using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Doctors;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctors;

public record GetDoctorsQuery : IRequest<ApiResponse<List<DoctorDto>>>;
