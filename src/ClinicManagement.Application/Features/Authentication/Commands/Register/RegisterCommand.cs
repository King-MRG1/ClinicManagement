using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Auth;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Authentication.Commands.Register;

public record RegisterCommand : IRequest<ApiResponse<AuthResponseDto>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string Address { get; init; } = string.Empty;
}
