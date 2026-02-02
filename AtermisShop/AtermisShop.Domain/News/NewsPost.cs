using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.News;

public class NewsPost : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid? AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public string? Url { get; set; }
}

