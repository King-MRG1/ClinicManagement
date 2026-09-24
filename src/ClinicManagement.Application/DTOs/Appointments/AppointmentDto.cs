using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.DTOs.Appointments;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
