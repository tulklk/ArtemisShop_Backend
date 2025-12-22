using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Products;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int ChangeQuantity { get; set; }
    public string? Reason { get; set; }
}

