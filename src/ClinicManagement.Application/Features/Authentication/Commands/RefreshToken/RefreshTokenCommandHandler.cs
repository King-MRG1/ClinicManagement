using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing refresh token request");

        var (succeeded, message, data) = await _identityService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken,
            cancellationToken);

        if (!succeeded || data == null)
        {
            _logger.LogWarning("Refresh token failed: {Message}", message);
            return ApiResponse<AuthResponseDto>.Failed(message);
        }

        return ApiResponse<AuthResponseDto>.Succeeded(data, "Token refreshed successfully.");
    }
}
