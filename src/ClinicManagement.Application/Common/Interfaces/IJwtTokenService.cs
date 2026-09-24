using System.Security.Claims;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, string role, Guid? doctorId = null, Guid? patientId = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
