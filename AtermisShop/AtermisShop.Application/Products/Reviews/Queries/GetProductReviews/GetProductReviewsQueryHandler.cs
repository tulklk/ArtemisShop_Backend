using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Reviews.Common;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, IReadOnlyList<ProductReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.ProductIdOrSlug, out var productId);
        
        var product = isGuid
            ? await _context.Products.FindAsync(new object[] { productId }, cancellationToken)
            : await _context.Products.FirstOrDefaultAsync(p => p.Slug == request.ProductIdOrSlug, cancellationToken);

        if (product == null)
            return Array.Empty<ProductReviewDto>();

        var reviews = await _context.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == product.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(r => new ProductReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            FullName = r.FullName ?? r.User?.FullName,
            PhoneNumber = r.PhoneNumber ?? r.User?.PhoneNumber,
            Email = r.Email ?? r.User?.Email,
            Rating = r.Rating,
            Comment = r.Comment,
            ReviewImageUrl = r.ReviewImageUrl,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
    }
}

