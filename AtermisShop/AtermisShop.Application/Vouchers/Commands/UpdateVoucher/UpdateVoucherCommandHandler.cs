using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Vouchers.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Vouchers.Commands.UpdateVoucher;

public sealed class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, VoucherDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherDto> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
        {
            throw new InvalidOperationException($"Voucher with ID {request.Id} not found");
        }

        // Validate discount type
        if (!DiscountTypeHelper.IsValid(request.DiscountType))
        {
            throw new ArgumentException($"Invalid discount type: {request.DiscountType}");
        }

        // Check code uniqueness (excluding current voucher)
        var existingVoucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.Code && v.Id != request.Id, cancellationToken);
        
        if (existingVoucher != null)
        {
            throw new InvalidOperationException($"Voucher with code '{request.Code}' already exists");
        }

        // Validate dates
        if (request.StartDate >= request.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        // Validate discount value
        if (request.DiscountValue <= 0)
        {
            throw new ArgumentException("Discount value must be greater than 0");
        }

        // Validate limits
        if (request.UsageLimitTotal < 0)
        {
            throw new ArgumentException("Usage limit total cannot be negative");
        }

        if (request.UsageLimitPerCustomer < 0)
        {
            throw new ArgumentException("Usage limit per customer cannot be negative");
        }

        // Validate percentage discount
        if (request.DiscountType == DiscountTypeHelper.Percent && request.DiscountValue > 100)
        {
            throw new ArgumentException("Percentage discount cannot exceed 100%");
        }

        // Validate used count doesn't exceed new limit
        if (voucher.UsedCount > request.UsageLimitTotal)
        {
            throw new ArgumentException($"Cannot set usage limit total below current used count ({voucher.UsedCount})");
        }

        // Update voucher
        voucher.Code = request.Code;
        voucher.Name = request.Name;
        voucher.Description = request.Description;
        voucher.DiscountType = DiscountTypeHelper.ToInt(request.DiscountType);
        voucher.DiscountValue = request.DiscountValue;
        voucher.MaxDiscountAmount = request.MaxDiscountAmount;
        voucher.MinOrderAmount = request.MinOrderAmount;
        voucher.StartDate = request.StartDate;
        voucher.EndDate = request.EndDate;
        voucher.IsPublic = request.IsPublic;
        voucher.UsageLimitTotal = request.UsageLimitTotal;
        voucher.UsageLimitPerCustomer = request.UsageLimitPerCustomer;

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

