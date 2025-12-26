using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Orders.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public CreateOrderCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate payment method
        if (!AtermisShop.Application.Orders.Common.PaymentMethod.IsValid(request.PaymentMethod))
        {
            throw new ArgumentException($"Invalid payment method. Only 'COD' and 'PayOS' are supported.");
        }

        // Get cart from database directly to access domain entities
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
            
        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Cart is empty");

        var orderNumber = await OrderNumberHelper.GenerateUniqueOrderNumberAsync(_context, cancellationToken);
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = request.UserId,
            OrderStatus = (int)OrderStatus.Pending,
            PaymentStatus = 0, // Pending
            PaymentMethod = AtermisShop.Application.Orders.Common.PaymentMethod.ToInt(request.PaymentMethod), // 0: COD, 1: PayOS
            ShippingFullName = request.ShippingAddress?.FullName,
            ShippingPhoneNumber = request.ShippingAddress?.PhoneNumber,
            ShippingAddressLine = request.ShippingAddress?.AddressLine,
            ShippingWard = null, // Removed from API to match Vietnam provinces API v2
            ShippingDistrict = request.ShippingAddress?.District,
            ShippingCity = request.ShippingAddress?.City
        };

        decimal subTotal = 0;
        foreach (var cartItem in cart.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { cartItem.ProductId }, cancellationToken);
            if (product == null)
                continue;

            var unitPrice = product.Price;
            var lineTotal = unitPrice * cartItem.Quantity;
            subTotal += lineTotal;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = cartItem.Quantity,
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

        // Clear cart after order creation
        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }
}

