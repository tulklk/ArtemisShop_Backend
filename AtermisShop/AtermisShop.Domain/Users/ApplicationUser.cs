using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Domain.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public string? GoogleId { get; set; }
    public string? Avatar { get; set; }
    public bool EmailVerified { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


