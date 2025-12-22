using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery() : IRequest<IReadOnlyList<Product>>;


