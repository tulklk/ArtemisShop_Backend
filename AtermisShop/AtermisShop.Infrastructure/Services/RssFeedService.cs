using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.News;

namespace AtermisShop.Infrastructure.Services;

public class RssFeedService : IRssFeedService
{
    private readonly HttpClient _httpClient;
    private static readonly string[] ChildAbductionKeywords = new[]
    {
        // Bắt cóc
        "bắt cóc trẻ em", "bắt cóc bé", "bắt cóc cháu", "bắt cóc con",
        "bắt cóc em bé", "bắt cóc trẻ", "bắt cóc học sinh", "bắt cóc",
        
        // Mất tích
        "mất tích trẻ em", "mất tích bé", "mất tích cháu",
        "bé gái mất tích", "bé trai mất tích", "trẻ em mất tích",
        
        // Buôn bán
        "buôn bán trẻ em", "buôn người", "bán trẻ em", "mua bán trẻ em",
        
        // Dụ dỗ & lừa đảo
        "dụ dỗ trẻ em", "dụ dỗ bé", "lừa đảo trẻ em", "dụ dỗ",
        
        // Xâm hại
        "xâm hại trẻ em", "xâm hại tình dục trẻ em", "xâm hại bé",
        "hiếp dâm trẻ em", "cưỡng hiếp trẻ em", "dâm ô trẻ em",
        "quấy rối tình dục trẻ em", "giao cấu trẻ em",
        
        // Tìm kiếm & giải cứu
        "tìm kiếm trẻ em", "giải cứu trẻ em", "tìm thấy bé",
        
        // Các từ liên quan đến trẻ em + tội phạm
        "bé gái", "bé trai", "em bé", "cháu bé",
        "trẻ em bị", "bé bị", "cháu bị"
    };

    public RssFeedService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RssArticle>> FetchArticlesAsync(string rssUrl, CancellationToken cancellationToken = default)
    {
        var articles = new List<RssArticle>();

        try
        {
            var response = await _httpClient.GetAsync(rssUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var xmlReader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items)
            {
                var article = new RssArticle
                {
                    Id = Guid.NewGuid(),
                    Title = item.Title?.Text ?? string.Empty,
                    Description = StripHtml(item.Summary?.Text ?? string.Empty),
                    Link = item.Links.FirstOrDefault()?.Uri.ToString() ?? string.Empty,
                    PublishedDate = item.PublishDate.DateTime,
                    ImageUrl = ExtractImageUrl(item),
                    Source = ExtractSourceName(rssUrl),
                    Category = ExtractCategoryFromUrl(rssUrl),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                articles.Add(article);
            }
        }
        catch (Exception ex)
        {
            // Log error here
            throw new Exception($"Error fetching RSS feed: {ex.Message}", ex);
        }

        return articles;
    }

    public async Task<List<RssArticle>> FetchAndFilterChildAbductionArticlesAsync(string rssUrl, CancellationToken cancellationToken = default)
    {
        var allArticles = await FetchArticlesAsync(rssUrl, cancellationToken);
        var filteredArticles = new List<RssArticle>();

        foreach (var article in allArticles)
        {
            var relevanceScore = CalculateRelevanceScore(article);
            
            if (relevanceScore > 0)
            {
                article.IsRelevantToChildAbduction = true;
                article.RelevanceScore = relevanceScore;
                filteredArticles.Add(article);
            }
        }

        return filteredArticles.OrderByDescending(a => a.RelevanceScore).ToList();
    }

    public async Task<List<RssArticle>> FetchFromMultipleSourcesAsync(CancellationToken cancellationToken = default)
    {
        var rssSources = new[]
        {
            "https://vnexpress.net/rss/phap-luat.rss",
            "https://vnexpress.net/rss/thoi-su.rss",
            "https://vnexpress.net/rss/giao-duc.rss",
            // Tuổi Trẻ uses different RSS structure
            "https://tuoitre.vn/rss/phap-luat.rss",
            "https://tuoitre.vn/rss/thoi-su.rss",
            // Thanh Niên
            "https://thanhnien.vn/rss/xa-hoi.rss",
            // Dân Trí
            "https://dantri.com.vn/rss/xa-hoi.rss",
            "https://dantri.com.vn/rss/phap-luat.rss"
        };

        var allArticles = new List<RssArticle>();
        var successCount = 0;
        var failCount = 0;
        
        var tasks = rssSources.Select(async url =>
        {
            try
            {
                var articles = await FetchAndFilterChildAbductionArticlesAsync(url, cancellationToken);
                if (articles.Any())
                {
                    Interlocked.Increment(ref successCount);
                    Console.WriteLine($"✓ Success: {url} - Found {articles.Count} articles");
                }
                else
                {
                    Console.WriteLine($"○ Success but no matches: {url}");
                }
                return articles;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failCount);
                Console.WriteLine($"✗ Failed: {url} - {ex.Message}");
                return new List<RssArticle>();
            }
        });

        var results = await Task.WhenAll(tasks);
        
        foreach (var articles in results)
        {
            allArticles.AddRange(articles);
        }

        Console.WriteLine($"Summary: {successCount} sources with matches, {failCount} failed");

        // Deduplicate by link
        var uniqueArticles = allArticles
            .GroupBy(a => a.Link)
            .Select(g => g.First())
            .OrderByDescending(a => a.RelevanceScore)
            .ToList();

        return uniqueArticles;
    }

    private double CalculateRelevanceScore(RssArticle article)
    {
        var score = 0.0;
        var combinedText = $"{article.Title} {article.Description}".ToLower();
        
        // High priority keywords (very specific - always include)
        var highPriorityKeywords = new[]
        {
            "bắt cóc trẻ em", "bắt cóc bé", "bắt cóc cháu", "bắt cóc con",
            "mất tích trẻ em", "mất tích bé", "buôn bán trẻ em",
            "xâm hại trẻ em", "xâm hại tình dục trẻ em", "hiếp dâm trẻ em",
            "dâm ô trẻ em", "cưỡng hiếp trẻ em", "giao cấu"
        };
        
        // Medium priority - child + crime context
        var mediumPriorityKeywords = new[]
        {
            "bé gái bị", "bé trai bị", "em bé bị", "cháu bé bị",
            "trẻ em bị", "bé bị", "cháu bị",
            "xâm hại bé", "dụ dỗ bé", "hãm hại bé",
            "bé gái 12", "bé gái 13", "bé gái 14", "bé gái 15",
            "bé trai 12", "bé trai 13", "bé trai 14", "bé trai 15"
        };
        
        // Check high priority keywords
        foreach (var keyword in highPriorityKeywords)
        {
            if (combinedText.Contains(keyword.ToLower()))
            {
                score += article.Title.ToLower().Contains(keyword.ToLower()) ? 5.0 : 3.0;
            }
        }
        
        // Check medium priority keywords
        foreach (var keyword in mediumPriorityKeywords)
        {
            if (combinedText.Contains(keyword.ToLower()))
            {
                score += article.Title.ToLower().Contains(keyword.ToLower()) ? 3.0 : 2.0;
            }
        }
        
        // If already has score, return
        if (score > 0)
        {
            return score;
        }

        // Check general keywords with relaxed context
        foreach (var keyword in ChildAbductionKeywords)
        {
            if (combinedText.Contains(keyword.ToLower()))
            {
                // For very general keywords, still need some context
                if (keyword == "bé gái" || keyword == "bé trai" || keyword == "em bé" || keyword == "cháu bé")
                {
                    // More relaxed context check
                    if (combinedText.Contains("bị") || combinedText.Contains("mất") || 
                        combinedText.Contains("tìm") || combinedText.Contains("xâm hại") ||
                        combinedText.Contains("dụ dỗ") || combinedText.Contains("lừa") ||
                        combinedText.Contains("bỏ nhà") || combinedText.Contains("đi lạc") ||
                        combinedText.Contains("hãm hại") || combinedText.Contains("quấy rối") ||
                        combinedText.Contains("cứu") || combinedText.Contains("vùi") ||
                        combinedText.Contains("sát hại") || combinedText.Contains("giết"))
                    {
                        score += article.Title.ToLower().Contains(keyword.ToLower()) ? 1.5 : 0.8;
                    }
                }
                else
                {
                    // Other keywords get points
                    score += article.Title.ToLower().Contains(keyword.ToLower()) ? 2.0 : 1.0;
                }
            }
        }

        return score;
    }

    private string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Remove HTML tags
        var withoutTags = Regex.Replace(html, "<.*?>", string.Empty);
        
        // Decode HTML entities
        var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);
        
        return decoded.Trim();
    }

    private string? ExtractImageUrl(SyndicationItem item)
    {
        // Try to extract image from enclosure
        var imageEnclosure = item.Links.FirstOrDefault(l => 
            l.RelationshipType == "enclosure" && 
            l.MediaType?.StartsWith("image/") == true);

        if (imageEnclosure != null)
        {
            return imageEnclosure.Uri.ToString();
        }

        // Try to extract from content
        var content = item.Summary?.Text ?? string.Empty;
        var imgMatch = Regex.Match(content, @"<img[^>]+src=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        
        if (imgMatch.Success)
        {
            return imgMatch.Groups[1].Value;
        }

        return null;
    }

    private string ExtractSourceName(string rssUrl)
    {
        if (rssUrl.Contains("vnexpress.net")) return "VnExpress";
        if (rssUrl.Contains("tuoitre.vn")) return "Tuổi Trẻ";
        if (rssUrl.Contains("thanhnien.vn")) return "Thanh Niên";
        if (rssUrl.Contains("dantri.com.vn")) return "Dân Trí";
        return "Unknown";
    }

    private string ExtractCategoryFromUrl(string rssUrl)
    {
        if (rssUrl.Contains("phap-luat")) return "Pháp luật";
        if (rssUrl.Contains("thoi-su")) return "Thời sự";
        if (rssUrl.Contains("xa-hoi")) return "Xã hội";
        if (rssUrl.Contains("giao-duc")) return "Giáo dục";
        return "Tin tức";
    }
}
