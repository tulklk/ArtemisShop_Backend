using AtermisShop.Application.Products.Comments.Common;
using MediatR;

namespace AtermisShop.Application.Products.Comments.Commands.ReplyProductComment;

public sealed record ReplyProductCommentCommand(
    Guid UserId,
    string Slug,
    Guid ParentCommentId,
    string Content) : IRequest<ProductCommentDto>;

