namespace ClinicManagement.Application.DTOs.Prescriptions;

public class PrescriptionDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = new();
}
