namespace ClinicManagement.Domain.Entities;

public class MedicalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string BloodPressure { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
