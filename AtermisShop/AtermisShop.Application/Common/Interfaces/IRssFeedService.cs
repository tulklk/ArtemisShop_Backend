using AtermisShop.Domain.News;

namespace AtermisShop.Application.Common.Interfaces;

public interface IRssFeedService
{
    Task<List<RssArticle>> FetchArticlesAsync(string rssUrl, CancellationToken cancellationToken = default);
    Task<List<RssArticle>> FetchAndFilterChildAbductionArticlesAsync(string rssUrl, CancellationToken cancellationToken = default);
    Task<List<RssArticle>> FetchFromMultipleSourcesAsync(CancellationToken cancellationToken = default);
}
