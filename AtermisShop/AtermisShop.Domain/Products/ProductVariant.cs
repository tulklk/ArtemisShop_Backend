using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Products;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Spec { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

