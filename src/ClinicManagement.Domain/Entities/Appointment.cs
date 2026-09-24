using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public DateTime AppointmentDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MedicalRecord? MedicalRecord { get; set; }
    public Prescription? Prescription { get; set; }
}
