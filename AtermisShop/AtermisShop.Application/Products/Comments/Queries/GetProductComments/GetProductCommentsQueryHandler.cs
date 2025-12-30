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

        var comments = await _context.ProductComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.ProductId == product.Id && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(c => MapToDto(c)).ToList();
    }

    private ProductCommentDto MapToDto(ProductComment comment)
    {
        return new ProductCommentDto
        {
            Id = comment.Id,
            ProductId = comment.ProductId,
            UserId = comment.UserId,
            UserName = comment.User?.FullName ?? "Unknown",
            UserAvatar = comment.User?.Avatar,
            IsAdmin = comment.User?.Role == 1, // Assuming 1 is Admin role
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            IsEdited = comment.IsEdited,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Replies = comment.Replies.OrderBy(r => r.CreatedAt).Select(r => MapToDto(r)).ToList()
        };
    }
}

