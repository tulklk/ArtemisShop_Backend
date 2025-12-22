using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;

public sealed class GetProductByIdOrSlugQueryHandler : IRequestHandler<GetProductByIdOrSlugQuery, Product?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdOrSlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> Handle(GetProductByIdOrSlugQuery request, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(request.IdOrSlug, out var id))
        {
            return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == request.IdOrSlug, cancellationToken);
    }
}


