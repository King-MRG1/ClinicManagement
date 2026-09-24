using ClinicManagement.Application.DTOs.Auth;
using MediatR;

namespace ClinicManagement.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
