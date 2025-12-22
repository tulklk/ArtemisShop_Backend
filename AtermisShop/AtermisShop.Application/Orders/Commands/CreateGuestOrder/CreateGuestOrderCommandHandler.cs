using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Commands.CreateGuestOrder;

public sealed class CreateGuestOrderCommandHandler : IRequestHandler<CreateGuestOrderCommand, Order>
{
    private readonly IApplicationDbContext _context;

    public CreateGuestOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order> Handle(CreateGuestOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = Guid.NewGuid();
        var order = new Order
        {
            OrderNumber = orderNumber,
            GuestEmail = request.GuestEmail,
            GuestFullName = request.GuestName,
            OrderStatus = (int)OrderStatus.Pending,
            PaymentStatus = 0, // Pending
            ShippingFullName = request.GuestName,
            ShippingPhoneNumber = request.GuestPhone,
            ShippingAddressLine = request.ShippingAddress
        };

        decimal subTotal = 0;
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
            if (product == null)
                continue;

            var unitPrice = product.Price;
            var lineTotal = unitPrice * item.Quantity;
            subTotal += lineTotal;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                ProductNameSnapshot = product.Name
            };
            order.Items.Add(orderItem);
        }

        // Apply voucher if provided
        if (!string.IsNullOrEmpty(request.VoucherCode))
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == request.VoucherCode && 
                    v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow && 
                    v.UsedCount < v.UsageLimitTotal, cancellationToken);

            if (voucher != null && subTotal >= (voucher.MinOrderAmount ?? 0))
            {
                decimal discount = 0;
                if (voucher.DiscountType == 1 && voucher.DiscountValue > 0) // Percentage
                {
                    discount = subTotal * (voucher.DiscountValue / 100);
                    if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
                        discount = voucher.MaxDiscountAmount.Value;
                }
                else if (voucher.DiscountType == 0) // Fixed amount
                {
                    discount = voucher.DiscountValue;
                }

                order.VoucherDiscountAmount = discount;
                order.VoucherId = voucher.Id;
            }
        }

        var shippingFee = 30000m; // Default shipping fee
        order.TotalAmount = subTotal + shippingFee - (order.VoucherDiscountAmount ?? 0);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }
}

