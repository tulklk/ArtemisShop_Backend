using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Products;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? ReviewImageUrl { get; set; }
}

