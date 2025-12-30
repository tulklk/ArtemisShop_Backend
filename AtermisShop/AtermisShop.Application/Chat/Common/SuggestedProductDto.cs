namespace AtermisShop.Application.Chat.Common;

public sealed class SuggestedProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int StockQuantity { get; set; }
}

