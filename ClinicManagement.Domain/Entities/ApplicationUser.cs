using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}
