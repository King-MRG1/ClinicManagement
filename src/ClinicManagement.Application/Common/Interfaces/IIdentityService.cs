using ClinicManagement.Application.DTOs.Auth;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors, ApplicationUser? User, Guid? PatientId)> RegisterPatientAsync(
        string email,
        string password,
        string fullName,
        string phoneNumber,
        DateOnly dateOfBirth,
        ClinicManagement.Domain.Enums.Gender gender,
        string address,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string Message, AuthResponseDto? Data)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string Message, AuthResponseDto? Data)> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        CancellationToken cancellationToken = default);
}
