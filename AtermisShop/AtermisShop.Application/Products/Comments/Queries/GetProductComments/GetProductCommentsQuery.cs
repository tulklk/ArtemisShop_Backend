using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Comments.Queries.GetProductComments;

public sealed record GetProductCommentsQuery(string ProductIdOrSlug) : IRequest<IReadOnlyList<ProductComment>>;

