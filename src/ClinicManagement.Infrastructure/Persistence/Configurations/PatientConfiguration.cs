using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Gender)
            .IsRequired();

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.MedicalRecords)
            .WithOne(m => m.Patient)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Prescriptions)
            .WithOne(pr => pr.Patient)
            .HasForeignKey(pr => pr.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
