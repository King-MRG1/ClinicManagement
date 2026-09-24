using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, string role, Guid? doctorId = null, Guid? patientId = null);
}
