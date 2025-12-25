using AtermisShop.Application.Products.Commands.CreateProduct;

namespace AtermisShop.Application.Products.Common;

public sealed class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; }
    public bool HasVariants { get; set; }
    public Guid CategoryId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

