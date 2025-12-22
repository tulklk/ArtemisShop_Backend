using MediatR;

namespace AtermisShop.Application.Products.Comments.Commands.DeleteProductComment;

public sealed record DeleteProductCommentCommand(Guid CommentId, Guid UserId) : IRequest<Unit>;

