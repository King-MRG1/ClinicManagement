using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    public string GenerateAccessToken(ApplicationUser user, string role, Guid? doctorId = null, Guid? patientId = null)
    {
        var secret = _configuration["Jwt:Key"] ?? _configuration["JwtSettings:Secret"] ?? "SuperSecretClinicManagementDefaultKey_1234567890!@#$";
        var issuer = _configuration["Jwt:Issuer"] ?? _configuration["JwtSettings:Issuer"] ?? "ClinicManagement";
        var audience = _configuration["Jwt:Audience"] ?? _configuration["JwtSettings:Audience"] ?? "ClinicManagement";
        var expiryMinutes = double.TryParse(_configuration["Jwt:ExpiryMinutes"] ?? _configuration["JwtSettings:ExpiryMinutes"], out var exp) ? exp : 60;

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

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secret = _configuration["Jwt:Key"] ?? _configuration["JwtSettings:Secret"] ?? "SuperSecretClinicManagementDefaultKey_1234567890!@#$";
        var issuer = _configuration["Jwt:Issuer"] ?? _configuration["JwtSettings:Issuer"] ?? "ClinicManagement";
        var audience = _configuration["Jwt:Audience"] ?? _configuration["JwtSettings:Audience"] ?? "ClinicManagement";

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
