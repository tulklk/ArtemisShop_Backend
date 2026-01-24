using System.ServiceModel.Syndication;
using System.Xml;

namespace RssTest;

class Program
{
    static async Task Main(string[] args)
    {
        var rssUrl = "https://vnexpress.net/rss/phap-luat.rss";
        
        Console.WriteLine($"Fetching RSS from: {rssUrl}");
        Console.WriteLine();
        
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(rssUrl);
        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var xmlReader = XmlReader.Create(stream);
        var feed = SyndicationFeed.Load(xmlReader);
        
        Console.WriteLine($"Feed Title: {feed.Title.Text}");
        Console.WriteLine($"Total Items: {feed.Items.Count()}");
        Console.WriteLine();
        
        var keywords = new[]
        {
            "bắt cóc", "mất tích", "xâm hại", "bé gái", "bé trai",
            "trẻ em", "giao cấu", "dâm ô", "hiếp dâm"
        };
        
        int matchCount = 0;
        foreach (var item in feed.Items)
        {
            var title = item.Title?.Text ?? "";
            var description = item.Summary?.Text ?? "";
            var combined = $"{title} {description}".ToLower();
            
            var matched = keywords.Any(k => combined.Contains(k.ToLower()));
            
            if (matched)
            {
                matchCount++;
                Console.WriteLine($"✓ MATCH #{matchCount}:");
                Console.WriteLine($"  Title: {title}");
                Console.WriteLine($"  Link: {item.Links.FirstOrDefault()?.Uri}");
                
                var matchedKeywords = keywords.Where(k => combined.Contains(k.ToLower())).ToList();
                Console.WriteLine($"  Keywords: {string.Join(", ", matchedKeywords)}");
                Console.WriteLine();
            }
        }
        
        Console.WriteLine($"Total matches: {matchCount} out of {feed.Items.Count()}");
    }
}
