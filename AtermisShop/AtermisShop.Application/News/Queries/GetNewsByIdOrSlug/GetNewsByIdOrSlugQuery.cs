using AtermisShop.Application.News.Common;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetNewsByIdOrSlug;

public sealed record GetNewsByIdOrSlugQuery(string IdOrSlug, bool IncrementViewCount = true) : IRequest<NewsDto?>;

