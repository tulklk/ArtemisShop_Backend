using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Vouchers.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Vouchers.Commands.CreateVoucher;

public sealed class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, VoucherDto>
{
    private readonly IApplicationDbContext _context;

    public CreateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherDto> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        // Validate discount type
        if (!DiscountTypeHelper.IsValid(request.DiscountType))
        {
            throw new ArgumentException($"Invalid discount type: {request.DiscountType}");
        }

        // Check code uniqueness
        var existingVoucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.Code, cancellationToken);
        
        if (existingVoucher != null)
        {
            throw new InvalidOperationException($"Voucher with code '{request.Code}' already exists");
        }

        // Validate dates
        if (request.StartDate >= request.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        if (request.EndDate < DateTime.UtcNow)
        {
            throw new ArgumentException("End date cannot be in the past");
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

        var voucher = new Voucher
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            DiscountType = DiscountTypeHelper.ToInt(request.DiscountType),
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinOrderAmount = request.MinOrderAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsPublic = request.IsPublic,
            UsageLimitTotal = request.UsageLimitTotal,
            UsageLimitPerCustomer = request.UsageLimitPerCustomer,
            UsedCount = 0
        };

        _context.Vouchers.Add(voucher);
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

