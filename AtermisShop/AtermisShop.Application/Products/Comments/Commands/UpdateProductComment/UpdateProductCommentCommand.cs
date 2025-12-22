using MediatR;

namespace AtermisShop.Application.Products.Comments.Commands.UpdateProductComment;

public sealed record UpdateProductCommentCommand(
    Guid CommentId,
    Guid UserId,
    string Content) : IRequest<Unit>;

