using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Specialization)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique();

        builder.HasIndex(d => d.UserId)
            .IsUnique();

        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.MedicalRecords)
            .WithOne(m => m.Doctor)
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Prescriptions)
            .WithOne(p => p.Doctor)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
