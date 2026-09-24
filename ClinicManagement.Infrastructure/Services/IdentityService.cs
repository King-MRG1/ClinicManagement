using System.Security.Claims;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.Auth;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IClinicDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IClinicDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<(bool Succeeded, string[] Errors, ApplicationUser? User, Guid? PatientId)> RegisterPatientAsync(
        string email,
        string password,
        string fullName,
        string phoneNumber,
        DateOnly dateOfBirth,
        Gender gender,
        string address,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return (false, new[] { "User with this email already exists." }, null, null);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName,
            PhoneNumber = phoneNumber
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray(), null, null);
        }

        await _userManager.AddToRoleAsync(user, "Patient");

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Address = address
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registered new patient {PatientId} with user {UserId}", patient.Id, user.Id);
        return (true, Array.Empty<string>(), user, patient.Id);
    }

    public async Task<(bool Succeeded, string Message, AuthResponseDto? Data)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, "Invalid email or password.", null);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return (false, "Invalid email or password.", null);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";

        Guid? doctorId = null;
        Guid? patientId = null;

        if (role == "Doctor")
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id, cancellationToken);
            doctorId = doctor?.Id;
        }
        else if (role == "Patient")
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
            patientId = patient?.Id;
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user, role, doctorId, patientId);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
        await _userManager.UpdateAsync(user);

        var data = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiryTime,
            UserId = user.Id,
            Email = user.Email ?? email,
            FullName = user.FullName,
            Role = role,
            DoctorId = doctorId,
            PatientId = patientId
        };

        return (true, "Login successful.", data);
    }

    public async Task<(bool Succeeded, string Message, AuthResponseDto? Data)> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return (false, "Invalid access token or refresh token.", null);
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return (false, "Invalid token claims.", null);
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return (false, "Invalid or expired refresh token.", null);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";

        Guid? doctorId = null;
        Guid? patientId = null;

        if (role == "Doctor")
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id, cancellationToken);
            doctorId = doctor?.Id;
        }
        else if (role == "Patient")
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
            patientId = patient?.Id;
        }

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, role, doctorId, patientId);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newExpiryTime = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = newExpiryTime;
        await _userManager.UpdateAsync(user);

        var data = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiryTime = newExpiryTime,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = role,
            DoctorId = doctorId,
            PatientId = patientId
        };

        return (true, "Token refreshed successfully.", data);
    }
}
