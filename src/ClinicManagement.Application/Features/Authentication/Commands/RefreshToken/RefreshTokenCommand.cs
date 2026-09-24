using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Auth;
using MediatR;

namespace ClinicManagement.Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<ApiResponse<AuthResponseDto>>
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
