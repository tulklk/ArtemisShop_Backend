using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Queries.LookupGuestOrder;

public sealed class LookupGuestOrderQueryHandler : IRequestHandler<LookupGuestOrderQuery, Order?>
{
    private readonly IApplicationDbContext _context;

    public LookupGuestOrderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> Handle(LookupGuestOrderQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Voucher)
            .Where(o => o.OrderNumber == request.OrderNumber && o.UserId == null);

        if (!string.IsNullOrEmpty(request.Email))
        {
            query = query.Where(o => o.GuestEmail == request.Email);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}

