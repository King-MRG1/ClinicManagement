using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence;

public class ClinicDbSeeder
{
    private readonly ClinicDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<ClinicDbSeeder> _logger;

    public ClinicDbSeeder(
        ClinicDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<ClinicDbSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // 1. Seed Roles (Only 3 roles: Doctor, Receptionist, Patient. NO Admin role!)
        string[] roles = ["Doctor", "Receptionist", "Patient"];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // 2. Seed 2 Doctors
        var doc1User = await EnsureUserAsync("doctor.smith@clinic.com", "Password123!", "Dr. John Smith", "+1-555-0111", "Doctor");
        var doc2User = await EnsureUserAsync("doctor.johnson@clinic.com", "Password123!", "Dr. Sarah Johnson", "+1-555-0112", "Doctor");

        var doc1 = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doc1User.Id);
        if (doc1 == null)
        {
            doc1 = new Doctor
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserId = doc1User.Id,
                Specialization = "Cardiology",
                LicenseNumber = "DOC-CARD-1001"
            };
            _context.Doctors.Add(doc1);
        }

        var doc2 = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doc2User.Id);
        if (doc2 == null)
        {
            doc2 = new Doctor
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserId = doc2User.Id,
                Specialization = "Pediatrics",
                LicenseNumber = "DOC-PED-1002"
            };
            _context.Doctors.Add(doc2);
        }
        await _context.SaveChangesAsync(CancellationToken.None);

        // 3. Seed 2 Receptionists
        await EnsureUserAsync("alice.reception@clinic.com", "Password123!", "Alice Williams", "+1-555-0121", "Receptionist");
        await EnsureUserAsync("bob.reception@clinic.com", "Password123!", "Bob Miller", "+1-555-0122", "Receptionist");

        // 4. Seed 5 Patients
        var p1User = await EnsureUserAsync("michael.brown@patient.com", "Password123!", "Michael Brown", "+1-555-0101", "Patient");
        var p2User = await EnsureUserAsync("emily.davis@patient.com", "Password123!", "Emily Davis", "+1-555-0102", "Patient");
        var p3User = await EnsureUserAsync("david.wilson@patient.com", "Password123!", "David Wilson", "+1-555-0103", "Patient");
        var p4User = await EnsureUserAsync("olivia.taylor@patient.com", "Password123!", "Olivia Taylor", "+1-555-0104", "Patient");
        var p5User = await EnsureUserAsync("james.anderson@patient.com", "Password123!", "James Anderson", "+1-555-0105", "Patient");

        var p1 = await EnsurePatientAsync(Guid.Parse("33333333-3333-3333-3333-333333333331"), p1User.Id, new DateOnly(1985, 5, 12), Gender.Male, "123 Elm St, Springfield");
        var p2 = await EnsurePatientAsync(Guid.Parse("33333333-3333-3333-3333-333333333332"), p2User.Id, new DateOnly(1992, 8, 20), Gender.Female, "456 Oak Ave, Springfield");
        var p3 = await EnsurePatientAsync(Guid.Parse("33333333-3333-3333-3333-333333333333"), p3User.Id, new DateOnly(1978, 3, 15), Gender.Male, "789 Pine Rd, Springfield");
        var p4 = await EnsurePatientAsync(Guid.Parse("33333333-3333-3333-3333-333333333334"), p4User.Id, new DateOnly(2000, 11, 28), Gender.Female, "321 Maple Dr, Springfield");
        var p5 = await EnsurePatientAsync(Guid.Parse("33333333-3333-3333-3333-333333333335"), p5User.Id, new DateOnly(1965, 7, 4), Gender.Male, "654 Cedar Ln, Springfield");

        await _context.SaveChangesAsync(CancellationToken.None);

        // 5. Seed Appointments
        var appt1Id = Guid.Parse("44444444-4444-4444-4444-444444444441");
        var appt2Id = Guid.Parse("44444444-4444-4444-4444-444444444442");
        var appt3Id = Guid.Parse("44444444-4444-4444-4444-444444444443");
        var appt4Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        if (!await _context.Appointments.AnyAsync(a => a.Id == appt1Id))
        {
            var futureDate1 = DateTime.UtcNow.Date.AddDays(2).AddHours(10);
            _context.Appointments.Add(new Appointment
            {
                Id = appt1Id,
                DoctorId = doc1.Id,
                PatientId = p1.Id,
                AppointmentDateTime = futureDate1,
                EndDateTime = futureDate1.AddMinutes(30),
                Status = AppointmentStatus.Scheduled,
                Reason = "Routine cardiovascular checkup",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await _context.Appointments.AnyAsync(a => a.Id == appt2Id))
        {
            var pastDate = DateTime.UtcNow.Date.AddDays(-3).AddHours(14);
            _context.Appointments.Add(new Appointment
            {
                Id = appt2Id,
                DoctorId = doc1.Id,
                PatientId = p2.Id,
                AppointmentDateTime = pastDate,
                EndDateTime = pastDate.AddMinutes(30),
                Status = AppointmentStatus.Completed,
                Reason = "Chest tightness and palpitations",
                CreatedAt = pastDate.AddDays(-2)
            });
        }

        if (!await _context.Appointments.AnyAsync(a => a.Id == appt3Id))
        {
            var futureDate2 = DateTime.UtcNow.Date.AddDays(3).AddHours(11);
            _context.Appointments.Add(new Appointment
            {
                Id = appt3Id,
                DoctorId = doc2.Id,
                PatientId = p3.Id,
                AppointmentDateTime = futureDate2,
                EndDateTime = futureDate2.AddMinutes(30),
                Status = AppointmentStatus.Confirmed,
                Reason = "Annual pediatric wellness check for family dependent",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await _context.Appointments.AnyAsync(a => a.Id == appt4Id))
        {
            var pastDate2 = DateTime.UtcNow.Date.AddDays(-5).AddHours(9);
            _context.Appointments.Add(new Appointment
            {
                Id = appt4Id,
                DoctorId = doc2.Id,
                PatientId = p4.Id,
                AppointmentDateTime = pastDate2,
                EndDateTime = pastDate2.AddMinutes(30),
                Status = AppointmentStatus.Completed,
                Reason = "Persistent fever and throat pain",
                CreatedAt = pastDate2.AddDays(-1)
            });
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        // 6. Seed Medical Records
        var medRec1Id = Guid.Parse("55555555-5555-5555-5555-555555555551");
        if (!await _context.MedicalRecords.AnyAsync(m => m.Id == medRec1Id))
        {
            _context.MedicalRecords.Add(new MedicalRecord
            {
                Id = medRec1Id,
                DoctorId = doc1.Id,
                PatientId = p2.Id,
                AppointmentId = appt2Id,
                Symptoms = "Mild chest discomfort during exertion, shortness of breath",
                Diagnosis = "Stage 1 Essential Hypertension",
                Treatment = "Prescribed Lisinopril, recommended low-sodium diet and daily walking",
                Notes = "Follow up in 4 weeks. ECG showed normal sinus rhythm.",
                BloodPressure = "138/88",
                Temperature = 36.8m,
                CreatedAt = DateTime.UtcNow.Date.AddDays(-3).AddHours(14).AddMinutes(25)
            });
        }

        var medRec2Id = Guid.Parse("55555555-5555-5555-5555-555555555552");
        if (!await _context.MedicalRecords.AnyAsync(m => m.Id == medRec2Id))
        {
            _context.MedicalRecords.Add(new MedicalRecord
            {
                Id = medRec2Id,
                DoctorId = doc2.Id,
                PatientId = p4.Id,
                AppointmentId = appt4Id,
                Symptoms = "Sore throat, fatigue, elevated temperature of 38.5C",
                Diagnosis = "Acute Streptococcal Pharyngitis",
                Treatment = "Oral Amoxicillin course for 10 days, hydration and throat lozenges",
                Notes = "Throat swab rapid test positive for Group A Strep.",
                BloodPressure = "118/76",
                Temperature = 38.5m,
                CreatedAt = DateTime.UtcNow.Date.AddDays(-5).AddHours(9).AddMinutes(20)
            });
        }

        // 7. Seed Prescriptions
        var presc1Id = Guid.Parse("66666666-6666-6666-6666-666666666661");
        if (!await _context.Prescriptions.AnyAsync(p => p.Id == presc1Id))
        {
            var presc1 = new Prescription
            {
                Id = presc1Id,
                DoctorId = doc1.Id,
                PatientId = p2.Id,
                AppointmentId = appt2Id,
                Notes = "Take in the morning with a glass of water. Monitor blood pressure weekly.",
                CreatedAt = DateTime.UtcNow.Date.AddDays(-3).AddHours(14).AddMinutes(28)
            };

            presc1.PrescriptionItems.Add(new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = presc1Id,
                MedicineName = "Lisinopril",
                Dosage = "10mg",
                Frequency = "Once daily",
                Duration = "30 days",
                Instructions = "Take in the morning with food"
            });

            presc1.PrescriptionItems.Add(new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = presc1Id,
                MedicineName = "Aspirin (Cardio)",
                Dosage = "81mg",
                Frequency = "Once daily",
                Duration = "30 days",
                Instructions = "Take with dinner"
            });

            _context.Prescriptions.Add(presc1);
        }

        var presc2Id = Guid.Parse("66666666-6666-6666-6666-666666666662");
        if (!await _context.Prescriptions.AnyAsync(p => p.Id == presc2Id))
        {
            var presc2 = new Prescription
            {
                Id = presc2Id,
                DoctorId = doc2.Id,
                PatientId = p4.Id,
                AppointmentId = appt4Id,
                Notes = "Complete entire course of antibiotics even if feeling better.",
                CreatedAt = DateTime.UtcNow.Date.AddDays(-5).AddHours(9).AddMinutes(22)
            };

            presc2.PrescriptionItems.Add(new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = presc2Id,
                MedicineName = "Amoxicillin",
                Dosage = "500mg",
                Frequency = "Three times daily",
                Duration = "10 days",
                Instructions = "Take after meals every 8 hours"
            });

            presc2.PrescriptionItems.Add(new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = presc2Id,
                MedicineName = "Paracetamol",
                Dosage = "500mg",
                Frequency = "Every 6 hours as needed",
                Duration = "5 days",
                Instructions = "Take for pain or fever greater than 38.0C"
            });

            _context.Prescriptions.Add(presc2);
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Database seeded successfully!");
    }

    private async Task<ApplicationUser> EnsureUserAsync(string email, string password, string fullName, string phoneNumber, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true
            };
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
            await _userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private async Task<Patient> EnsurePatientAsync(Guid id, Guid userId, DateOnly dob, Gender gender, string address)
    {
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null)
        {
            patient = new Patient
            {
                Id = id,
                UserId = userId,
                DateOfBirth = dob,
                Gender = gender,
                Address = address
            };
            _context.Patients.Add(patient);
        }

        return patient;
    }
}
