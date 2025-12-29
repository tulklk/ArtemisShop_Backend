using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Vouchers.Commands.DeleteVoucher;

public sealed class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers
            .Include(v => v.Orders)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
        {
            throw new InvalidOperationException($"Voucher with ID {request.Id} not found");
        }

        // Check if voucher has been used in any orders
        if (voucher.Orders.Any())
        {
            throw new InvalidOperationException("Cannot delete voucher that has been used in orders. Voucher usage history must be preserved.");
        }

        // Delete voucher usages if any
        var usages = await _context.VoucherUsages
            .Where(u => u.VoucherId == request.Id)
            .ToListAsync(cancellationToken);

        if (usages.Any())
        {
            _context.VoucherUsages.RemoveRange(usages);
        }

        // Delete the voucher
        _context.Vouchers.Remove(voucher);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

