using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Comments.Common;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Commands.CreateProductComment;

public sealed class CreateProductCommentCommandHandler : IRequestHandler<CreateProductCommentCommand, ProductCommentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductCommentDto> Handle(CreateProductCommentCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Product not found");

        if (request.ParentCommentId.HasValue)
        {
            var parent = await _context.ProductComments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId && c.ProductId == product.Id, cancellationToken);
            if (parent == null)
                throw new InvalidOperationException("Parent comment not found");
        }

        var comment = new ProductComment
        {
            ProductId = product.Id,
            UserId = request.UserId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };

        _context.ProductComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload comment with user to map to DTO
        var commentWithUser = await _context.ProductComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken);

        return new ProductCommentDto
        {
            Id = commentWithUser!.Id,
            ProductId = commentWithUser.ProductId,
            UserId = commentWithUser.UserId,
            UserName = commentWithUser.User?.FullName ?? "Unknown",
            UserAvatar = commentWithUser.User?.Avatar,
            IsAdmin = commentWithUser.User?.Role == 1, // Assuming 1 is Admin role
            ParentCommentId = commentWithUser.ParentCommentId,
            Content = commentWithUser.Content,
            IsEdited = commentWithUser.IsEdited,
            CreatedAt = commentWithUser.CreatedAt,
            UpdatedAt = commentWithUser.UpdatedAt,
            Replies = new List<ProductCommentDto>()
        };
    }
}

