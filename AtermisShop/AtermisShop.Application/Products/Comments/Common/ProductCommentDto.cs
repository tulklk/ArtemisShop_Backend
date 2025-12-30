namespace AtermisShop.Application.Products.Comments.Common;

public sealed class ProductCommentDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string? UserAvatar { get; set; }
    public bool IsAdmin { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = default!;
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProductCommentDto> Replies { get; set; } = new();
}

