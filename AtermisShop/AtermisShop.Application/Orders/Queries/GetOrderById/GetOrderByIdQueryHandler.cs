using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Order?>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Voucher)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId);
        }

        return await query.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
    }
}

