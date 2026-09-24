using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Diagnosis)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Treatment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        builder.Property(m => m.VisitDate)
            .IsRequired();

        builder.HasOne(m => m.Doctor)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
