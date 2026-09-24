namespace ClinicManagement.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
}
