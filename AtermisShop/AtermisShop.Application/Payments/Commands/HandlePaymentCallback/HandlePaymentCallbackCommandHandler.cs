using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Payments.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Payments.Commands.HandlePaymentCallback;

public sealed class HandlePaymentCallbackCommandHandler : IRequestHandler<HandlePaymentCallbackCommand, Order?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;

    public HandlePaymentCallbackCommandHandler(
        IApplicationDbContext context,
        IEnumerable<IPaymentProvider> paymentProviders)
    {
        _context = context;
        _paymentProviders = paymentProviders;
    }

    public async Task<Order?> Handle(HandlePaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        var provider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
            return null;

        var callbackResult = await provider.VerifyCallbackAsync(request.CallbackData, cancellationToken);
        if (!callbackResult.Success)
            return null;

        // For PayOS, OrderId in callback result is actually the orderCode (string)
        // We need to find the order by PaymentTransactionId (which stores the orderCode)
        Order? order = null;
        
        if (request.Provider.Equals("PayOS", StringComparison.OrdinalIgnoreCase))
        {
            // PayOS returns orderCode as OrderId, find order by PaymentTransactionId
            order = await _context.Orders
                .FirstOrDefaultAsync(o => o.PaymentTransactionId == callbackResult.OrderId, cancellationToken);
        }
        else
        {
            // For other providers, try to parse as Guid
            if (Guid.TryParse(callbackResult.OrderId, out var orderId))
            {
                order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            }
        }

        if (order == null || order.OrderStatus != (int)OrderStatus.Pending)
            return null;

        // Update order status to Paid
        order.OrderStatus = (int)OrderStatus.Paid;
        order.PaymentStatus = 1; // Paid
        await _context.SaveChangesAsync(cancellationToken);

        return order;
    }
}

