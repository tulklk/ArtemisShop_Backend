using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Chat;

public class ChatMessage : BaseEntity
{
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string Message { get; set; } = default!;
    public string? SessionId { get; set; }
}

