using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, ApiResponse<DoctorDto>>
{
    private readonly IClinicDbContext _context;

    public GetDoctorByIdQueryHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DoctorDto>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.Id == request.Id)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                UserId = d.UserId,
                FullName = d.User.FullName,
                Email = d.User.Email ?? string.Empty,
                PhoneNumber = d.User.PhoneNumber ?? string.Empty,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.Id);
        }

        return ApiResponse<DoctorDto>.Succeeded(doctor);
    }
}
