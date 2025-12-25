using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using AtermisShop.Domain.Products;
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
        // Validate payment method
        if (!AtermisShop.Application.Orders.Common.PaymentMethod.IsValid(request.PaymentMethod))
        {
            throw new ArgumentException($"Invalid payment method. Only 'COD' and 'PayOS' are supported.");
        }

        var orderNumber = Guid.NewGuid();
        var order = new Order
        {
            OrderNumber = orderNumber,
            GuestEmail = request.Email,
            GuestFullName = request.FullName,
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
        foreach (var item in request.Items)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
            
            if (product == null)
                continue;

            decimal unitPrice;
            ProductVariant? variant = null;
            string? variantInfo = null;

            // If ProductVariantId is provided, use variant price
            if (item.ProductVariantId.HasValue)
            {
                variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId.Value && v.IsActive);
                if (variant != null)
                {
                    unitPrice = variant.Price;
                    // Build variant info string
                    var variantParts = new List<string>();
                    if (!string.IsNullOrEmpty(variant.Color)) variantParts.Add($"Màu: {variant.Color}");
                    if (!string.IsNullOrEmpty(variant.Size)) variantParts.Add($"Size: {variant.Size}");
                    if (!string.IsNullOrEmpty(variant.Spec)) variantParts.Add($"Spec: {variant.Spec}");
                    variantInfo = string.Join(", ", variantParts);
                }
                else
                {
                    // Variant not found or inactive, use product price
                    unitPrice = product.Price;
                }
            }
            else
            {
                // No variant specified, use product price
                unitPrice = product.Price;
            }

            var lineTotal = unitPrice * item.Quantity;
            subTotal += lineTotal;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                ProductVariantId = variant?.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                ProductNameSnapshot = product.Name,
                VariantInfoSnapshot = variantInfo
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

