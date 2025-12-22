using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ProductCategory?>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductCategory?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
    }
}

