using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(string? Keyword) : IRequest<IReadOnlyList<Product>>;


