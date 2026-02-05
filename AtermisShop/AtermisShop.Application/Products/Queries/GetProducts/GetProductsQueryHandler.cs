using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Common;
using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Variants)
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
            Images = p.Images.Select(img => new ProductImageDto
            {
                ImageUrl = img.ImageUrl,
                Type = (ProductImageTypeDto)img.Type
            }).ToList(),
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


