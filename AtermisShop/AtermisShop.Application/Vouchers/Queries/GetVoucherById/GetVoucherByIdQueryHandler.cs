using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Vouchers.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Vouchers.Queries.GetVoucherById;

public sealed class GetVoucherByIdQueryHandler : IRequestHandler<GetVoucherByIdQuery, VoucherDto?>
{
    private readonly IApplicationDbContext _context;

    public GetVoucherByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherDto?> Handle(GetVoucherByIdQuery request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
            return null;

        return new VoucherDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            DiscountType = DiscountTypeHelper.FromInt(voucher.DiscountType),
            DiscountValue = voucher.DiscountValue,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            MinOrderAmount = voucher.MinOrderAmount,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            IsPublic = voucher.IsPublic,
            UsageLimitTotal = voucher.UsageLimitTotal,
            UsageLimitPerCustomer = voucher.UsageLimitPerCustomer,
            UsedCount = voucher.UsedCount,
            CreatedAt = voucher.CreatedAt,
            UpdatedAt = voucher.UpdatedAt
        };
    }
}

