using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Comments.Common;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Queries.GetProductComments;

public sealed class GetProductCommentsQueryHandler : IRequestHandler<GetProductCommentsQuery, IReadOnlyList<ProductCommentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductCommentDto>> Handle(GetProductCommentsQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

        if (product == null)
            return Array.Empty<ProductCommentDto>();

        // Load all comments for this product with their users
        var allComments = await _context.ProductComments
            .Include(c => c.User)
            .Where(c => c.ProductId == product.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Build a dictionary for quick lookup: parentId -> list of replies
        var repliesDict = allComments
            .Where(c => c.ParentCommentId.HasValue)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedAt).ToList());

        // Get only top-level comments (no parent)
        var topLevelComments = allComments
            .Where(c => c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return topLevelComments.Select(c => MapToDtoRecursive(c, repliesDict)).ToList();
    }

    private ProductCommentDto MapToDtoRecursive(ProductComment comment, Dictionary<Guid, List<ProductComment>> repliesDict)
    {
        var replies = repliesDict.TryGetValue(comment.Id, out var commentReplies)
            ? commentReplies.Select(r => MapToDtoRecursive(r, repliesDict)).ToList()
            : new List<ProductCommentDto>();

        return new ProductCommentDto
        {
            Id = comment.Id,
            ProductId = comment.ProductId,
            UserId = comment.UserId,
            UserName = comment.User?.FullName ?? "Unknown",
            UserAvatar = comment.User?.Avatar,
            IsAdmin = comment.User?.Role == 1, // 1 is Admin role
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            IsEdited = comment.IsEdited,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Replies = replies
        };
    }
}

