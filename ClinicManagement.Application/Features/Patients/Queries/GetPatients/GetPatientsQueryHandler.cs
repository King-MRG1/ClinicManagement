using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<PatientDto>>
{
    private readonly IClinicDbContext _context;

    public GetPatientsQueryHandler(IClinicDbContext context)
    {
        _context = context;
    }

    public async Task<List<PatientDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.FullName.ToLower().Contains(term) || p.PhoneNumber.Contains(term));
        }

        return await query
            .OrderBy(p => p.FullName)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = p.FullName,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                DateOfBirth = p.DateOfBirth
            })
            .ToListAsync(cancellationToken);
    }
}
