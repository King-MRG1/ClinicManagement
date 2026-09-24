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
        _logger.LogInformation("Seeding initial data...");

        string[] roles = ["Doctor", "Receptionist", "Patient"];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // 1. Seed 1 Doctor
        var docUser = await EnsureUserAsync("doctor@clinic.com", "Password123!", "Dr. Alice Smith", "+1-555-0100", "Doctor");
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == docUser.Id);
        if (doctor == null)
        {
            doctor = new Doctor
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserId = docUser.Id,
                Specialization = "Cardiology"
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
        }

        // 2. Seed 1 Receptionist
        await EnsureUserAsync("receptionist@clinic.com", "Password123!", "Bob Receptionist", "+1-555-0101", "Receptionist");

        // 3. Seed 2 Patients
        var p1User = await EnsureUserAsync("patient1@clinic.com", "Password123!", "John Doe", "+1-555-0201", "Patient");
        var p2User = await EnsureUserAsync("patient2@clinic.com", "Password123!", "Jane Smith", "+1-555-0202", "Patient");

        var p1 = await EnsurePatientAsync(Guid.Parse("22222222-2222-2222-2222-222222222221"), p1User.Id, p1User.FullName, p1User.Email!, p1User.PhoneNumber!, new DateOnly(1990, 1, 15));
        var p2 = await EnsurePatientAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"), p2User.Id, p2User.FullName, p2User.Email!, p2User.PhoneNumber!, new DateOnly(1995, 5, 20));
        await _context.SaveChangesAsync();

        // 4. Seed 2 Appointments
        var appt1Id = Guid.Parse("33333333-3333-3333-3333-333333333331");
        if (!await _context.Appointments.AnyAsync(a => a.Id == appt1Id))
        {
            _context.Appointments.Add(new Appointment
            {
                Id = appt1Id,
                DoctorId = doctor.Id,
                PatientId = p1.Id,
                AppointmentDate = DateTime.UtcNow.Date.AddDays(2).AddHours(10),
                Status = AppointmentStatus.Scheduled
            });
        }

        var appt2Id = Guid.Parse("33333333-3333-3333-3333-333333333332");
        if (!await _context.Appointments.AnyAsync(a => a.Id == appt2Id))
        {
            _context.Appointments.Add(new Appointment
            {
                Id = appt2Id,
                DoctorId = doctor.Id,
                PatientId = p2.Id,
                AppointmentDate = DateTime.UtcNow.Date.AddDays(-2).AddHours(14),
                Status = AppointmentStatus.Completed
            });
        }
        await _context.SaveChangesAsync();

        // 5. Seed 1 Medical Record
        var recordId = Guid.Parse("44444444-4444-4444-4444-444444444441");
        if (!await _context.MedicalRecords.AnyAsync(m => m.Id == recordId))
        {
            _context.MedicalRecords.Add(new MedicalRecord
            {
                Id = recordId,
                DoctorId = doctor.Id,
                PatientId = p2.Id,
                Diagnosis = "Hypertension",
                Treatment = "Lifestyle modification and regular monitoring",
                Notes = "Blood pressure 135/85. Follow up in 1 month.",
                VisitDate = DateTime.UtcNow.Date.AddDays(-2).AddHours(14)
            });
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Database seeded successfully.");
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
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await _userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private async Task<Patient> EnsurePatientAsync(Guid id, Guid userId, string fullName, string email, string phone, DateOnly dob)
    {
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null)
        {
            patient = new Patient
            {
                Id = id,
                UserId = userId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                DateOfBirth = dob
            };
            _context.Patients.Add(patient);
        }

        return patient;
    }
}
