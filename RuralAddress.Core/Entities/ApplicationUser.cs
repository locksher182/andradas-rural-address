using Microsoft.AspNetCore.Identity;

namespace RuralAddress.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public bool MustChangePassword { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastAccessDate { get; set; }
}
