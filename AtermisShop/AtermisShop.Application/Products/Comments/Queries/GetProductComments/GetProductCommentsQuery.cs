using AtermisShop.Application.Products.Comments.Common;
using MediatR;

namespace AtermisShop.Application.Products.Comments.Queries.GetProductComments;

public sealed record GetProductCommentsQuery(string Slug) : IRequest<IReadOnlyList<ProductCommentDto>>;

