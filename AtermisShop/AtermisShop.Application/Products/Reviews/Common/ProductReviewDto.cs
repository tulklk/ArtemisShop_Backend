namespace AtermisShop.Application.Products.Reviews.Common;

public sealed class ProductReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? UserId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public string? ReviewImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

