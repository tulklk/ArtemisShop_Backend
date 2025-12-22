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
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.VoucherCode && 
                v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow, cancellationToken);

        if (voucher == null)
            return new ApplyVoucherResult(false, 0, "Voucher not found or expired");

        if (voucher.UsedCount >= voucher.UsageLimitTotal)
            return new ApplyVoucherResult(false, 0, "Voucher usage limit exceeded");

        if (voucher.MinOrderAmount.HasValue && request.OrderAmount < voucher.MinOrderAmount.Value)
            return new ApplyVoucherResult(false, 0, $"Minimum order amount is {voucher.MinOrderAmount.Value}");

        decimal discount = 0;
        if (voucher.DiscountType == 1 && voucher.DiscountValue > 0) // Percentage
        {
            discount = request.OrderAmount * (voucher.DiscountValue / 100);
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

