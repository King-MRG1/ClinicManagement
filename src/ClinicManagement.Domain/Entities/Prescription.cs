namespace ClinicManagement.Domain.Entities;

public class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
