using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Commands.DeleteProductComment;

public sealed class DeleteProductCommentCommandHandler : IRequestHandler<DeleteProductCommentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProductCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.ProductComments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && c.UserId == request.UserId, cancellationToken);

        if (comment == null)
            throw new InvalidOperationException("Comment not found");

        // Delete replies first
        _context.ProductComments.RemoveRange(comment.Replies);
        _context.ProductComments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

