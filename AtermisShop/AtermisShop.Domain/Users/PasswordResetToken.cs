using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Users;

public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}

