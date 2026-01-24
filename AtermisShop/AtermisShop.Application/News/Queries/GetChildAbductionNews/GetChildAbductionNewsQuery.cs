using AtermisShop.Application.News.Common;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetChildAbductionNews;

public record GetChildAbductionNewsQuery : IRequest<List<RssArticleDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 6;
}
