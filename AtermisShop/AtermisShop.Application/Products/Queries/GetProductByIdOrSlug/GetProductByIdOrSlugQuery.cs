using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;

public sealed record GetProductByIdOrSlugQuery(string IdOrSlug) : IRequest<Product?>;


