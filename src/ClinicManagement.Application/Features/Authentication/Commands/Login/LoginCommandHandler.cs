using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IIdentityService identityService,
        ILogger<LoginCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing login attempt for email {Email}", request.Email);

        var (succeeded, message, data) = await _identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!succeeded || data == null)
        {
            _logger.LogWarning("Login attempt failed for email {Email}: {Message}", request.Email, message);
            return ApiResponse<AuthResponseDto>.Failed(message);
        }

        _logger.LogInformation("Login successful for user {Email} with role {Role}", request.Email, data.Role);
        return ApiResponse<AuthResponseDto>.Succeeded(data, "Login successful.");
    }
}
