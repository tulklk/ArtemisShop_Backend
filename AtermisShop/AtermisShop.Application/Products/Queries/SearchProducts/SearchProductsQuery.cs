using AtermisShop.Application.Products.Common;
using MediatR;

namespace AtermisShop.Application.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(string? Keyword) : IRequest<IReadOnlyList<ProductDto>>;


