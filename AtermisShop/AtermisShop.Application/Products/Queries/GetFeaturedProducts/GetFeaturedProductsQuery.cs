using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetFeaturedProducts;

public sealed record GetFeaturedProductsQuery() : IRequest<IReadOnlyList<Product>>;


