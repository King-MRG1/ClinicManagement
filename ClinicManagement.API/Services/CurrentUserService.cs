using System.Security.Claims;
using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClinicDbContext _context;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IClinicDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public async Task<Guid?> GetCurrentDoctorIdAsync(CancellationToken cancellationToken = default)
    {
        var doctorIdClaim = User?.FindFirst("DoctorId")?.Value;
        if (Guid.TryParse(doctorIdClaim, out var doctorId))
        {
            return doctorId;
        }

        if (UserId.HasValue)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == UserId.Value, cancellationToken);
            return doctor?.Id;
        }

        return null;
    }

    public async Task<Guid?> GetCurrentPatientIdAsync(CancellationToken cancellationToken = default)
    {
        var patientIdClaim = User?.FindFirst("PatientId")?.Value;
        if (Guid.TryParse(patientIdClaim, out var patientId))
        {
            return patientId;
        }

        if (UserId.HasValue)
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == UserId.Value, cancellationToken);
            return patient?.Id;
        }

        return null;
    }
}
