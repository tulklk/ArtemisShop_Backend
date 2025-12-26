using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Orders.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Queries.GetMyOrders;

public sealed class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Voucher)
            .Include(o => o.User)
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserEmail = order.User?.Email,
            UserFullName = order.User?.FullName,
            GuestEmail = order.GuestEmail,
            GuestFullName = order.GuestFullName,
            TotalAmount = order.TotalAmount,
            PaymentStatus = order.PaymentStatus,
            OrderStatus = order.OrderStatus,
            PaymentMethod = order.PaymentMethod,
            PaymentTransactionId = order.PaymentTransactionId,
            ShippingFullName = order.ShippingFullName,
            ShippingPhoneNumber = order.ShippingPhoneNumber,
            ShippingAddressLine = order.ShippingAddressLine,
            ShippingWard = order.ShippingWard,
            ShippingDistrict = order.ShippingDistrict,
            ShippingCity = order.ShippingCity,
            VoucherId = order.VoucherId,
            VoucherCode = order.Voucher?.Code,
            VoucherDiscountAmount = order.VoucherDiscountAmount,
            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                ProductNameSnapshot = item.ProductNameSnapshot,
                VariantInfoSnapshot = item.VariantInfoSnapshot,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        }).ToList();
    }
}

