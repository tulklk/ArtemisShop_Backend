using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Queries.GetProductStatistics;

public sealed class GetProductStatisticsQueryHandler : IRequestHandler<GetProductStatisticsQuery, ProductStatisticsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductStatisticsDto?> Handle(GetProductStatisticsQuery request, CancellationToken cancellationToken)
    {
        // Check if product exists
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            return null;

        // Calculate total sold quantity (excluding canceled orders)
        var canceledOrderStatus = (int)OrderStatus.Canceled;
        var totalSold = await _context.OrderItems
            .Join(_context.Orders,
                oi => oi.OrderId,
                o => o.Id,
                (oi, o) => new { OrderItem = oi, Order = o })
            .Where(x => x.OrderItem.ProductId == request.ProductId && x.Order.OrderStatus != canceledOrderStatus)
            .SumAsync(x => x.OrderItem.Quantity, cancellationToken);

        return new ProductStatisticsDto
        {
            ProductId = request.ProductId,
            TotalSold = totalSold
        };
    }
}

