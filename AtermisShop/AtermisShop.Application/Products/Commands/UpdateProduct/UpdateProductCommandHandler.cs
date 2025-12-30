using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Product not found");

        // Update product properties
        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;

        // Update OriginalPrice: if null, product is normal (no discount), otherwise use provided value
        if (request.OriginalPrice.HasValue)
            product.OriginalPrice = request.OriginalPrice.Value;
        else
            product.OriginalPrice = null; // Normal product (no discount)

        if (request.StockQuantity.HasValue)
            product.StockQuantity = request.StockQuantity.Value;

        if (!string.IsNullOrEmpty(request.Brand))
            product.Brand = request.Brand;

        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

