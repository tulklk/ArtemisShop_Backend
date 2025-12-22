using AtermisShop.Domain.Common;
using AtermisShop.Domain.Products;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Wishlist;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
}

