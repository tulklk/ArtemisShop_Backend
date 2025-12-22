using AtermisShop.Domain.News;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetNews;

public sealed record GetNewsQuery() : IRequest<IReadOnlyList<NewsPost>>;

