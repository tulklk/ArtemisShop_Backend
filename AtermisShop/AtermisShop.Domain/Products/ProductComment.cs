using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Products;

public class ProductComment : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public Guid? ParentCommentId { get; set; }
    public ProductComment? ParentComment { get; set; }
    public ICollection<ProductComment> Replies { get; set; } = new List<ProductComment>();
    public string Content { get; set; } = default!;
    public bool IsEdited { get; set; }
}

