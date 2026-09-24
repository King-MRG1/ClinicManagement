using System.Security.Claims;
using ClinicManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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

    public Guid? DoctorId
    {
        get
        {
            var doctorIdClaim = User?.FindFirst("DoctorId")?.Value;
            return Guid.TryParse(doctorIdClaim, out var doctorId) ? doctorId : null;
        }
    }

    public Guid? PatientId
    {
        get
        {
            var patientIdClaim = User?.FindFirst("PatientId")?.Value;
            return Guid.TryParse(patientIdClaim, out var patientId) ? patientId : null;
        }
    }
}
