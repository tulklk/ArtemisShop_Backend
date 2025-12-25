using AtermisShop.Application.Products.Common;
using MediatR;

namespace AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;

public sealed record GetProductByIdOrSlugQuery(string IdOrSlug) : IRequest<ProductDto?>;


