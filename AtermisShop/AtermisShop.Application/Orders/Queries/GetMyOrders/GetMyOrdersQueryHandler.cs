using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Queries.GetMyOrders;

public sealed class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<Order>>
{
    private readonly IApplicationDbContext _context;

    public GetMyOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Voucher)
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

