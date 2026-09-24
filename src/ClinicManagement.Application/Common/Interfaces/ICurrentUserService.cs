namespace ClinicManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    Task<Guid?> GetCurrentDoctorIdAsync(CancellationToken cancellationToken = default);
    Task<Guid?> GetCurrentPatientIdAsync(CancellationToken cancellationToken = default);
}
