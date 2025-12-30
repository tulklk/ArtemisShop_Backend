using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Commands.DeleteProductComment;

public sealed class DeleteProductCommentCommandHandler : IRequestHandler<DeleteProductCommentCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserService _userService;

    public DeleteProductCommentCommandHandler(IApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<Unit> Handle(DeleteProductCommentCommand request, CancellationToken cancellationToken)
    {
        // First, find the comment
        var comment = await _context.ProductComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
            throw new InvalidOperationException("Comment not found");

        // Check if user is admin
        var user = await _userService.FindByIdAsync(request.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var isAdmin = await _userService.IsAdminAsync(user);

        // Allow deletion if: user is admin OR user owns the comment
        if (!isAdmin && comment.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own comments");

        // Load all comments for this product to find all nested replies
        var allProductComments = await _context.ProductComments
            .Where(c => c.ProductId == comment.ProductId)
            .ToListAsync(cancellationToken);

        // Collect all nested reply IDs recursively in memory
        var allReplyIds = new HashSet<Guid>();
        CollectAllReplyIdsRecursive(comment.Id, allProductComments, allReplyIds);

        // Get all comments to delete (including the main comment)
        var commentsToDelete = allProductComments
            .Where(c => c.Id == comment.Id || allReplyIds.Contains(c.Id))
            .ToList();

        // Delete all comments (nested replies first, then the main comment)
        _context.ProductComments.RemoveRange(commentsToDelete);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void CollectAllReplyIdsRecursive(Guid commentId, List<ProductComment> allComments, HashSet<Guid> collectedIds)
    {
        var directReplies = allComments
            .Where(c => c.ParentCommentId == commentId)
            .ToList();

        foreach (var reply in directReplies)
        {
            if (!collectedIds.Contains(reply.Id))
            {
                collectedIds.Add(reply.Id);
                // Recursively collect nested replies
                CollectAllReplyIdsRecursive(reply.Id, allComments, collectedIds);
            }
        }
    }
}

