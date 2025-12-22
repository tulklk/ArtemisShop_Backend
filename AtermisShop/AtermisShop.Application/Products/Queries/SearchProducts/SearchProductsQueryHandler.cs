using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, IReadOnlyList<Product>>
{
    private readonly IApplicationDbContext _context;

    public SearchProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(p => p.Name.Contains(request.Keyword) || (p.Description ?? "").Contains(request.Keyword));
        }

        return await query.ToListAsync(cancellationToken);
    }
}


