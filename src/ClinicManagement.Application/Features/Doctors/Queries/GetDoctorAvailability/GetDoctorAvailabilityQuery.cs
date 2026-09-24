using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Doctors;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorAvailability;

public record GetDoctorAvailabilityQuery(Guid DoctorId, DateOnly Date) : IRequest<ApiResponse<DoctorAvailabilityDto>>;
