using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Vouchers.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Vouchers.Commands.PublishVoucher;

public sealed class PublishVoucherCommandHandler : IRequestHandler<PublishVoucherCommand, VoucherDto>
{
    private readonly IApplicationDbContext _context;

    public PublishVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherDto> Handle(PublishVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
        {
            throw new InvalidOperationException($"Voucher with ID {request.Id} not found");
        }

        voucher.IsPublic = request.IsPublic;
        await _context.SaveChangesAsync(cancellationToken);

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

