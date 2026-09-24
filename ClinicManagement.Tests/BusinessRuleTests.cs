using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;
using ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecords;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicManagement.Tests;

public class BusinessRuleTests
{
    private class FakeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? PatientId { get; set; }
    }

    private static ClinicDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ClinicDbContext(options);
    }

    [Fact]
    public async Task Doctor_CannotHaveTwoAppointmentsAtSameTime_ThrowsConflictException()
    {
        // Arrange
        using var context = CreateDbContext();
        var doctorId = Guid.NewGuid();
        var patient1Id = Guid.NewGuid();
        var patient2Id = Guid.NewGuid();
        var appointmentTime = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);

        var docUser = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Dr. Test" };
        var patUser1 = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Patient 1" };
        var patUser2 = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Patient 2" };

        var doctor = new Doctor { Id = doctorId, UserId = docUser.Id, User = docUser, Specialization = "General" };
        var patient1 = new Patient { Id = patient1Id, UserId = patUser1.Id, User = patUser1, FullName = "Patient 1", DateOfBirth = new DateOnly(1990, 1, 1) };
        var patient2 = new Patient { Id = patient2Id, UserId = patUser2.Id, User = patUser2, FullName = "Patient 2", DateOfBirth = new DateOnly(1995, 1, 1) };

        context.Doctors.Add(doctor);
        context.Patients.AddRange(patient1, patient2);
        context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = patient1Id,
            AppointmentDate = appointmentTime,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var handler = new CreateAppointmentCommandHandler(context);
        var command = new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patient2Id,
            AppointmentDate = appointmentTime
        };

        // Act & Assert: A doctor cannot have two appointments at the exact same date and time.
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Patient_CanOnlySeeOwnMedicalRecords()
    {
        // Arrange
        using var context = CreateDbContext();
        var doctorId = Guid.NewGuid();
        var patient1Id = Guid.NewGuid();
        var patient2Id = Guid.NewGuid();

        var docUser = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Dr. Test" };
        var patUser1 = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Patient 1" };
        var patUser2 = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Patient 2" };

        var doctor = new Doctor { Id = doctorId, UserId = docUser.Id, User = docUser };
        var patient1 = new Patient { Id = patient1Id, UserId = patUser1.Id, User = patUser1, FullName = "Patient 1", DateOfBirth = new DateOnly(1990, 1, 1) };
        var patient2 = new Patient { Id = patient2Id, UserId = patUser2.Id, User = patUser2, FullName = "Patient 2", DateOfBirth = new DateOnly(1995, 1, 1) };

        context.Doctors.Add(doctor);
        context.Patients.AddRange(patient1, patient2);

        // Record for Patient 1
        context.MedicalRecords.Add(new MedicalRecord
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = patient1Id,
            Diagnosis = "Flu",
            Treatment = "Rest",
            VisitDate = DateTime.UtcNow
        });

        // Record for Patient 2
        context.MedicalRecords.Add(new MedicalRecord
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = patient2Id,
            Diagnosis = "Fracture",
            Treatment = "Cast",
            VisitDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var currentUserService = new FakeCurrentUserService
        {
            Role = "Patient",
            PatientId = patient1Id
        };

        var handler = new GetMedicalRecordsQueryHandler(context, currentUserService);

        // Act: Even if requesting patient2's id, patient must only receive their own records
        var result = await handler.Handle(new GetMedicalRecordsQuery(patient2Id), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(patient1Id, result[0].PatientId);
        Assert.Equal("Flu", result[0].Diagnosis);
    }

    [Fact]
    public async Task Patient_CannotCreateMedicalRecords_ThrowsForbiddenException()
    {
        // Arrange
        using var context = CreateDbContext();
        var currentUserService = new FakeCurrentUserService
        {
            Role = "Patient",
            PatientId = Guid.NewGuid()
        };

        var handler = new CreateMedicalRecordCommandHandler(context, currentUserService);
        var command = new CreateMedicalRecordCommand
        {
            PatientId = currentUserService.PatientId.Value,
            Diagnosis = "Self-diagnosis",
            Treatment = "Self-treatment"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
    }
}
