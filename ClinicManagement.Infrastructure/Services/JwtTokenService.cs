using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClinicManagement.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, string role, Guid? doctorId = null, Guid? patientId = null)
    {
        var secret = _configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("Configuration 'Jwt:Key' is not configured.");
        var issuer = _configuration["Jwt:Issuer"] 
            ?? throw new InvalidOperationException("Configuration 'Jwt:Issuer' is not configured.");
        var audience = _configuration["Jwt:Audience"] 
            ?? throw new InvalidOperationException("Configuration 'Jwt:Audience' is not configured.");
        var expiryMinutesStr = _configuration["Jwt:ExpiryMinutes"] 
            ?? throw new InvalidOperationException("Configuration 'Jwt:ExpiryMinutes' is not configured.");

        if (!double.TryParse(expiryMinutesStr, out var expiryMinutes))
        {
            expiryMinutes = 60;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (doctorId.HasValue)
        {
            claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
        }

        if (patientId.HasValue)
        {
            claims.Add(new Claim("PatientId", patientId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
