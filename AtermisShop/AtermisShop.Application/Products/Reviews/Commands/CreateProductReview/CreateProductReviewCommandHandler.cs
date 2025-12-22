using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Reviews.Commands.CreateProductReview;

public sealed class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.ProductIdOrSlug, out var productId);
        
        var product = isGuid
            ? await _context.Products.FindAsync(new object[] { productId }, cancellationToken)
            : await _context.Products.FirstOrDefaultAsync(p => p.Slug == request.ProductIdOrSlug, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Product not found");

        var review = new ProductReview
        {
            ProductId = product.Id,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        return review.Id;
    }
}

