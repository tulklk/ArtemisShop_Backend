using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Commands.CreateProductComment;

public sealed class CreateProductCommentCommandHandler : IRequestHandler<CreateProductCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommentCommand request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.ProductIdOrSlug, out var productId);
        
        var product = isGuid
            ? await _context.Products.FindAsync(new object[] { productId }, cancellationToken)
            : await _context.Products.FirstOrDefaultAsync(p => p.Slug == request.ProductIdOrSlug, cancellationToken);

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

        return comment.Id;
    }
}

