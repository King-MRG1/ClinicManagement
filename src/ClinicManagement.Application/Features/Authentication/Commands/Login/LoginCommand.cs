using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Auth;
using MediatR;

namespace ClinicManagement.Application.Features.Authentication.Commands.Login;

public record LoginCommand : IRequest<ApiResponse<AuthResponseDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
