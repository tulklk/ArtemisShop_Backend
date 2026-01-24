using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.News.Common;
using AtermisShop.Domain.News;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetChildAbductionNews;

public class GetChildAbductionNewsQueryHandler : IRequestHandler<GetChildAbductionNewsQuery, List<RssArticleDto>>
{
    private readonly IRssFeedService _rssFeedService;

    public GetChildAbductionNewsQueryHandler(IRssFeedService rssFeedService)
    {
        _rssFeedService = rssFeedService;
    }

    public async Task<List<RssArticleDto>> Handle(GetChildAbductionNewsQuery request, CancellationToken cancellationToken)
    {
        // Always fetch from all predefined sources
        var articles = await _rssFeedService.FetchFromMultipleSourcesAsync(cancellationToken);

        // Filter out articles without images
        articles = articles.Where(a => !string.IsNullOrWhiteSpace(a.ImageUrl)).ToList();

        // Calculate pagination
        var totalCount = articles.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        
        // Apply pagination
        var paginatedArticles = articles
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = paginatedArticles.Select(a => new RssArticleDto
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            Link = a.Link,
            PublishedDate = a.PublishedDate,
            ImageUrl = a.ImageUrl,
            Source = a.Source,
            Category = a.Category,
            IsRelevantToChildAbduction = a.IsRelevantToChildAbduction,
            RelevanceScore = a.RelevanceScore,
            TotalArticlesFetched = totalCount
        }).ToList();

        return dtos;
    }
}
