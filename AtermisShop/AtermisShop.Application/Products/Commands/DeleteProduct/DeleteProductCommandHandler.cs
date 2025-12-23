using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Product not found");

        // Delete related entities
        if (product.Variants.Any())
            _context.ProductVariants.RemoveRange(product.Variants);

        if (product.Images.Any())
            _context.ProductImages.RemoveRange(product.Images);

        if (product.Specifications.Any())
            _context.ProductSpecifications.RemoveRange(product.Specifications);

        // Delete reviews and comments related to this product
        var reviews = await _context.ProductReviews
            .Where(r => r.ProductId == product.Id)
            .ToListAsync(cancellationToken);
        if (reviews.Any())
            _context.ProductReviews.RemoveRange(reviews);

        var comments = await _context.ProductComments
            .Where(c => c.ProductId == product.Id)
            .ToListAsync(cancellationToken);
        if (comments.Any())
            _context.ProductComments.RemoveRange(comments);

        // Delete the product
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

