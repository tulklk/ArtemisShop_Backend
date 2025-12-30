namespace AtermisShop.Application.Products.Common;

public sealed class ProductStatisticsDto
{
    public Guid ProductId { get; set; }
    public int TotalSold { get; set; }
}

