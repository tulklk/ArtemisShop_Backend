using AtermisShop.Application.Products.Common;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetFeaturedProducts;

public sealed record GetFeaturedProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;


