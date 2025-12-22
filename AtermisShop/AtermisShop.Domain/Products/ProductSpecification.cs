using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Products;

public class ProductSpecification : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public int DisplayOrder { get; set; }
}

