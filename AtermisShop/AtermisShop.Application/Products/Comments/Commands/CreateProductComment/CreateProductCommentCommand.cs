using MediatR;

namespace AtermisShop.Application.Products.Comments.Commands.CreateProductComment;

public sealed record CreateProductCommentCommand(
    Guid UserId,
    string ProductIdOrSlug,
    string Content,
    Guid? ParentCommentId = null) : IRequest<Guid>;

