using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<Product>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products.AsNoTracking().ToListAsync(cancellationToken);
    }
}


