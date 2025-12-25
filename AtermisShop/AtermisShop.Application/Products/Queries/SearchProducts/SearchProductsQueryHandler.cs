using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Common;
using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(p => p.Name.Contains(request.Keyword) || (p.Description ?? "").Contains(request.Keyword));
        }

        var products = await query.ToListAsync(cancellationToken);

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


