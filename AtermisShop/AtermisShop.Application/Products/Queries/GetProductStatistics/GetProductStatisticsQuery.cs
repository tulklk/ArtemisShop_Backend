using AtermisShop.Application.Products.Common;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetProductStatistics;

public sealed record GetProductStatisticsQuery(Guid ProductId) : IRequest<ProductStatisticsDto?>;

