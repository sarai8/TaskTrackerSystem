using Microsoft.AspNetCore.Identity;

namespace TaskTrackerSystem.Infrastructure.Persistence;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string AvatarId { get; set; } = "avatar-1";

    public int? DepartmentId { get; set; }
}