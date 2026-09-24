using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IClinicDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
