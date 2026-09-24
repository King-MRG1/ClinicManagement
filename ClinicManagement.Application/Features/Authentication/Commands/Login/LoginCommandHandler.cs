using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs.Auth;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IClinicDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IClinicDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
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

        var token = _jwtTokenService.GenerateToken(user, role, doctorId, patientId);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Role = role,
            DoctorId = doctorId,
            PatientId = patientId
        };
    }
}
