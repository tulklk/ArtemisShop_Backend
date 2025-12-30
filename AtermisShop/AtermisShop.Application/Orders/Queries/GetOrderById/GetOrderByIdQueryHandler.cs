using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Orders.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Voucher)
            .Include(o => o.User)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId);
        }

        var order = await query
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return null;

        return new OrderDto
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
                LineTotal = item.LineTotal,
                EngravingText = item.EngravingText
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}

