using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetFeaturedProducts;

public sealed class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, IReadOnlyList<Product>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);
    }
}


