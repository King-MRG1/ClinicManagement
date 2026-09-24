using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IClinicDbContext
{
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
