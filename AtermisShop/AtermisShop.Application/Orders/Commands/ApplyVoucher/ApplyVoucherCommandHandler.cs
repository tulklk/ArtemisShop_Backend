using AtermisShop.Application.Common.Interfaces;
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
        
        // If OrderAmount is provided, use it (for guest orders)
        if (request.OrderAmount.HasValue)
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

