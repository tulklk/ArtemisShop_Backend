using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Reviews.Common;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Reviews.Commands.CreateProductReview;

public sealed class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, ProductReviewDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProductReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductReviewDto> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
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
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Rating = request.Rating,
            Comment = request.Comment,
            ReviewImageUrl = request.ReviewImageUrl
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload review with user if exists to get additional info
        var reviewWithUser = await _context.ProductReviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == review.Id, cancellationToken);

        return new ProductReviewDto
        {
            Id = reviewWithUser!.Id,
            ProductId = reviewWithUser.ProductId,
            UserId = reviewWithUser.UserId,
            FullName = reviewWithUser.FullName ?? reviewWithUser.User?.FullName,
            PhoneNumber = reviewWithUser.PhoneNumber ?? reviewWithUser.User?.PhoneNumber,
            Email = reviewWithUser.Email ?? reviewWithUser.User?.Email,
            Rating = reviewWithUser.Rating,
            Comment = reviewWithUser.Comment,
            ReviewImageUrl = reviewWithUser.ReviewImageUrl,
            CreatedAt = reviewWithUser.CreatedAt,
            UpdatedAt = reviewWithUser.UpdatedAt
        };
    }
}

