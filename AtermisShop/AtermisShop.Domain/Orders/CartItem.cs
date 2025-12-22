using AtermisShop.Domain.Common;
using AtermisShop.Domain.Products;

namespace AtermisShop.Domain.Orders;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
}

