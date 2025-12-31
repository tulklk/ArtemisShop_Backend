using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Common;
using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetFeaturedProducts;

public sealed class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            StockQuantity = p.StockQuantity,
            Brand = p.Brand,
            IsActive = p.IsActive,
            HasVariants = p.HasVariants,
            HasEngraving = p.HasEngraving,
            DefaultEngravingText = p.DefaultEngravingText,
            Model3DUrl = p.Model3DUrl,
            CategoryId = p.CategoryId,
            ImageUrls = p.Images.Select(img => img.ImageUrl).ToList(),
            Variants = p.Variants.Select(v => new ProductVariantDto(
                v.Color,
                v.Size,
                v.Spec,
                v.Price,
                v.OriginalPrice,
                v.StockQuantity,
                v.IsActive
            )).ToList()
        }).ToList();
    }
}


