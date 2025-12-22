using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Orders;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

