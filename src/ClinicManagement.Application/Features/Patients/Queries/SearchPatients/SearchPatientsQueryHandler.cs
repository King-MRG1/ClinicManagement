using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.SearchPatients;

public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, ApiResponse<List<PatientDto>>>
{
    private readonly IClinicDbContext _context;

    public SearchPatientsQueryHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<PatientDto>>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.User.FullName.ToLower().Contains(term) ||
                (p.User.Email != null && p.User.Email.ToLower().Contains(term)) ||
                (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(term)) ||
                p.Address.ToLower().Contains(term));
        }

        var patients = await query
            .Select(p => new PatientDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = p.User.FullName,
                Email = p.User.Email ?? string.Empty,
                PhoneNumber = p.User.PhoneNumber ?? string.Empty,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                Address = p.Address
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<PatientDto>>.Succeeded(patients);
    }
}
