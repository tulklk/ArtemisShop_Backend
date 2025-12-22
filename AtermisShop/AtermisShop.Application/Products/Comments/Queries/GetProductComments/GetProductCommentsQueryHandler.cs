using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Comments.Queries.GetProductComments;

public sealed class GetProductCommentsQueryHandler : IRequestHandler<GetProductCommentsQuery, IReadOnlyList<ProductComment>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductComment>> Handle(GetProductCommentsQuery request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.ProductIdOrSlug, out var productId);
        
        var product = isGuid
            ? await _context.Products.FindAsync(new object[] { productId }, cancellationToken)
            : await _context.Products.FirstOrDefaultAsync(p => p.Slug == request.ProductIdOrSlug, cancellationToken);

        if (product == null)
            return Array.Empty<ProductComment>();

        return await _context.ProductComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.ProductId == product.Id && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

