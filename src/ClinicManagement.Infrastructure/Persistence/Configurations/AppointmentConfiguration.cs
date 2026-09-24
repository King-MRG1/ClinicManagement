using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.AppointmentDateTime)
            .IsRequired();

        builder.Property(a => a.EndDateTime)
            .IsRequired();

        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDateTime });
        builder.HasIndex(a => new { a.PatientId, a.AppointmentDateTime });

        builder.HasOne(a => a.MedicalRecord)
            .WithOne(m => m.Appointment)
            .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Prescription)
            .WithOne(p => p.Appointment)
            .HasForeignKey<Prescription>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
