using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.News;

public class RssArticle : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Link { get; set; } = default!;
    public DateTime PublishedDate { get; set; }
    public string? ImageUrl { get; set; }
    public string Source { get; set; } = default!;
    public string Category { get; set; } = default!;
    public bool IsRelevantToChildAbduction { get; set; }
    public double RelevanceScore { get; set; }
}
