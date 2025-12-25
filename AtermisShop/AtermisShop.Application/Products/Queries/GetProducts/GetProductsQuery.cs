using AtermisShop.Application.Products.Common;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;


