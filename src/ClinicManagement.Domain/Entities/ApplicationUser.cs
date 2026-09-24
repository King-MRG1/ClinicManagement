using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}
