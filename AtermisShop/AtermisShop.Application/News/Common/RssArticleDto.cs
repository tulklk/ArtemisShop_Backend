namespace AtermisShop.Application.News.Common;

public class RssArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Link { get; set; } = default!;
    public DateTime PublishedDate { get; set; }
    public string? ImageUrl { get; set; }
    public string Source { get; set; } = default!;
    public string Category { get; set; } = default!;
    public bool IsRelevantToChildAbduction { get; set; }
    public double RelevanceScore { get; set; }
    
    // Debug info
    public int? TotalArticlesFetched { get; set; }
    public List<string>? MatchedKeywords { get; set; }
}
