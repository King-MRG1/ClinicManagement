using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterPatientAsync(
            request.Email,
            request.Password,
            request.FullName,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Gender,
            request.Address,
            cancellationToken);

        if (!result.Succeeded || result.User == null)
        {
            _logger.LogWarning("Patient registration failed for email {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", result.Errors));

            return ApiResponse<AuthResponseDto>.Failed("Registration failed.", result.Errors);
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(
            result.User,
            "Patient",
            null,
            result.PatientId);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        result.User.RefreshToken = refreshToken;
        result.User.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        _logger.LogInformation("Patient account created successfully for email {Email}", request.Email);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = result.User.RefreshTokenExpiryTime.Value,
            UserId = result.User.Id,
            Email = result.User.Email ?? request.Email,
            FullName = result.User.FullName,
            Role = "Patient",
            PatientId = result.PatientId
        };

        return ApiResponse<AuthResponseDto>.Succeeded(response, "Registration successful.");
    }
}
