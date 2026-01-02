using AtermisShop.Application.News.Common;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetNews;

public sealed record GetNewsQuery(
    int? Page = null,
    int? PageSize = null,
    string? Category = null,
    string? Search = null) : IRequest<IReadOnlyList<NewsDto>>;

