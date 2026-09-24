namespace ClinicManagement.Application.DTOs.Doctors;

public class TimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public class DoctorAvailabilityDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public List<TimeSlotDto> Slots { get; set; } = new();
}
