using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Products;

public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
    public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}


