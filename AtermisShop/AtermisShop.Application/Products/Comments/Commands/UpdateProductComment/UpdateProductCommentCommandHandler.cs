using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Commands.UpdateProductComment;

public sealed class UpdateProductCommentCommandHandler : IRequestHandler<UpdateProductCommentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateProductCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.ProductComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && c.UserId == request.UserId, cancellationToken);

        if (comment == null)
            throw new InvalidOperationException("Comment not found");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

