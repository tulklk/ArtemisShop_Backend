using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, IReadOnlyList<ProductReview>>
{
    private readonly IApplicationDbContext _context;

    public GetProductReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductReview>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.ProductIdOrSlug, out var productId);
        
        var product = isGuid
            ? await _context.Products.FindAsync(new object[] { productId }, cancellationToken)
            : await _context.Products.FirstOrDefaultAsync(p => p.Slug == request.ProductIdOrSlug, cancellationToken);

        if (product == null)
            return Array.Empty<ProductReview>();

        return await _context.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == product.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

