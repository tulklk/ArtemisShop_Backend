namespace AtermisShop.Application.Wishlist.Common;

public sealed class WishlistDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? ProductDescription { get; set; }
    public string? ProductImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public bool IsActive { get; set; }
    public bool HasVariants { get; set; }
    public DateTime AddedAt { get; set; }
}

