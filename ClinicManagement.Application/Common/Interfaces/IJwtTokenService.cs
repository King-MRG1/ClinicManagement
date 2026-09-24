using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, string role, Guid? doctorId = null, Guid? patientId = null);
}
