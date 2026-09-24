namespace ClinicManagement.Application.DTOs.MedicalRecords;

public class MedicalRecordDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string BloodPressure { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public DateTime CreatedAt { get; set; }
}
