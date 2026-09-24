using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctors;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, ApiResponse<List<DoctorDto>>>
{
    private readonly IClinicDbContext _context;

    public GetDoctorsQueryHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<DoctorDto>>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
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
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DoctorDto>>.Succeeded(doctors);
    }
}
