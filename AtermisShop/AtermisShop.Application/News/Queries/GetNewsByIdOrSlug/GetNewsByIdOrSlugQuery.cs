using AtermisShop.Domain.News;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetNewsByIdOrSlug;

public sealed record GetNewsByIdOrSlugQuery(string IdOrSlug) : IRequest<NewsPost?>;

