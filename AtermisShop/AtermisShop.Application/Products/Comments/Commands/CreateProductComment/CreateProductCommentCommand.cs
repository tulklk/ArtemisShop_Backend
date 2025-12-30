using AtermisShop.Application.Products.Comments.Common;
using MediatR;

namespace AtermisShop.Application.Products.Comments.Commands.CreateProductComment;

public sealed record CreateProductCommentCommand(
    Guid UserId,
    string Slug,
    string Content,
    Guid? ParentCommentId = null) : IRequest<ProductCommentDto>;

