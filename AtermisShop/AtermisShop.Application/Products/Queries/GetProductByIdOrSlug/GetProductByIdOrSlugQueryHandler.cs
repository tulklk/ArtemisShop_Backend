using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Common;
using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;

public sealed class GetProductByIdOrSlugQueryHandler : IRequestHandler<GetProductByIdOrSlugQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdOrSlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(GetProductByIdOrSlugQuery request, CancellationToken cancellationToken)
    {
        Product? product;
        
        if (Guid.TryParse(request.IdOrSlug, out var id))
        {
            product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        else
        {
            product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Slug == request.IdOrSlug, cancellationToken);
        }

        if (product == null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            StockQuantity = product.StockQuantity,
            Brand = product.Brand,
            IsActive = product.IsActive,
            HasVariants = product.HasVariants,
            HasEngraving = product.HasEngraving,
            DefaultEngravingText = product.DefaultEngravingText,
            Model3DUrl = product.Model3DUrl,
            CategoryId = product.CategoryId,
            ImageUrls = product.Images.Select(img => img.ImageUrl).ToList(),
            Variants = product.Variants.Select(v => new ProductVariantDto(
                v.Color,
                v.Size,
                v.Spec,
                v.Price,
                v.OriginalPrice,
                v.StockQuantity,
                v.IsActive
            )).ToList()
        };
    }
}


