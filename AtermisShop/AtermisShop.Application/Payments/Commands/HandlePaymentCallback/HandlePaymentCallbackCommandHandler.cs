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

        if (!Guid.TryParse(callbackResult.OrderId, out var orderId))
            return null;

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null || order.OrderStatus != (int)OrderStatus.Pending)
            return null;

        // Update order status to Paid
        order.OrderStatus = (int)OrderStatus.Paid;
        order.PaymentStatus = 1; // Paid
        await _context.SaveChangesAsync(cancellationToken);

        return order;
    }
}

