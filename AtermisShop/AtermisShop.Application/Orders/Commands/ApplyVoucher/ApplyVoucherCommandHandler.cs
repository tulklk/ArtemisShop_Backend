using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Commands.ApplyVoucher;

public sealed class ApplyVoucherCommandHandler : IRequestHandler<ApplyVoucherCommand, ApplyVoucherResult>
{
    private readonly IApplicationDbContext _context;

    public ApplyVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplyVoucherResult> Handle(ApplyVoucherCommand request, CancellationToken cancellationToken)
    {
        decimal orderAmount = 0;
        
        // If GuestItems is provided, calculate order amount from items
        if (request.GuestItems != null && request.GuestItems.Any())
        {
            foreach (var item in request.GuestItems)
            {
                if (item.Quantity <= 0)
                {
                    return new ApplyVoucherResult(false, 0, $"Invalid quantity for product {item.ProductId}. Quantity must be greater than 0.");
                }

                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
                
                if (product == null)
                {
                    return new ApplyVoucherResult(false, 0, $"Product with ID {item.ProductId} not found.");
                }

                if (!product.IsActive)
                {
                    return new ApplyVoucherResult(false, 0, $"Product {product.Name} is not active.");
                }

                decimal unitPrice;

                // If ProductVariantId is provided, use variant price
                if (item.ProductVariantId.HasValue)
                {
                    var variant = product.Variants?.FirstOrDefault(v => v.Id == item.ProductVariantId.Value && v.IsActive);
                    if (variant != null)
                    {
                        unitPrice = variant.Price;
                    }
                    else
                    {
                        return new ApplyVoucherResult(false, 0, $"Product variant with ID {item.ProductVariantId.Value} not found or inactive.");
                    }
                }
                else
                {
                    // No variant specified, use product price
                    unitPrice = product.Price;
                }

                orderAmount += unitPrice * item.Quantity;
            }
        }
        // If OrderAmount is provided, use it (for guest orders with pre-calculated amount)
        else if (request.OrderAmount.HasValue)
        {
            orderAmount = request.OrderAmount.Value;
        }
        // Otherwise, get from cart (for authenticated users)
        else if (request.UserId.HasValue)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId.Value, cancellationToken);

            if (cart != null && cart.Items.Any())
            {
                orderAmount = cart.Items.Sum(item => item.Quantity * item.UnitPriceSnapshot);
            }
        }

        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.Code && 
                v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow, cancellationToken);

        if (voucher == null)
            return new ApplyVoucherResult(false, 0, "Voucher not found or expired");

        if (voucher.UsedCount >= voucher.UsageLimitTotal)
            return new ApplyVoucherResult(false, 0, "Voucher usage limit exceeded");

        if (voucher.MinOrderAmount.HasValue && orderAmount < voucher.MinOrderAmount.Value)
            return new ApplyVoucherResult(false, 0, $"Minimum order amount is {voucher.MinOrderAmount.Value}");

        decimal discount = 0;
        if (voucher.DiscountType == 1 && voucher.DiscountValue > 0) // Percentage
        {
            discount = orderAmount * (voucher.DiscountValue / 100);
            if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
                discount = voucher.MaxDiscountAmount.Value;
        }
        else if (voucher.DiscountType == 0) // Fixed amount
        {
            discount = voucher.DiscountValue;
        }

        return new ApplyVoucherResult(true, discount);
    }
}

