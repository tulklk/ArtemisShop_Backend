using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Cart.Queries.GetCart;

public sealed class GetCartQueryHandler : IRequestHandler<GetCartQuery, Domain.Orders.Cart?>
{
    private readonly IApplicationDbContext _context;

    public GetCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Orders.Cart?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
    }
}

