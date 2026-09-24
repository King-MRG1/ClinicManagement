namespace ClinicManagement.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    Guid? DoctorId { get; }
    Guid? PatientId { get; }
}
