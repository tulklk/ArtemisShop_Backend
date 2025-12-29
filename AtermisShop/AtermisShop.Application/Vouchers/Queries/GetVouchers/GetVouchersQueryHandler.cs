using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Vouchers.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Vouchers.Queries.GetVouchers;

public sealed class GetVouchersQueryHandler : IRequestHandler<GetVouchersQuery, IReadOnlyList<VoucherDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVouchersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VoucherDto>> Handle(GetVouchersQuery request, CancellationToken cancellationToken)
    {
        var vouchers = await _context.Vouchers
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return vouchers.Select(v => new VoucherDto
        {
            Id = v.Id,
            Code = v.Code,
            Name = v.Name,
            Description = v.Description,
            DiscountType = DiscountTypeHelper.FromInt(v.DiscountType),
            DiscountValue = v.DiscountValue,
            MaxDiscountAmount = v.MaxDiscountAmount,
            MinOrderAmount = v.MinOrderAmount,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            IsPublic = v.IsPublic,
            UsageLimitTotal = v.UsageLimitTotal,
            UsageLimitPerCustomer = v.UsageLimitPerCustomer,
            UsedCount = v.UsedCount,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt
        }).ToList();
    }
}

