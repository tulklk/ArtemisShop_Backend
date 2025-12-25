using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Commands.UpdateOrderPaymentTransaction;

public sealed class UpdateOrderPaymentTransactionCommandHandler : IRequestHandler<UpdateOrderPaymentTransactionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderPaymentTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateOrderPaymentTransactionCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return false;

        order.PaymentTransactionId = request.PaymentTransactionId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

