using AtermisShop.Application.News.Queries.GetNewsById;
using MediatR;

namespace AtermisShop.Application.News.Queries.GetAllNews;

public sealed record GetAllNewsQuery() : IRequest<IReadOnlyList<NewsDto>>;

