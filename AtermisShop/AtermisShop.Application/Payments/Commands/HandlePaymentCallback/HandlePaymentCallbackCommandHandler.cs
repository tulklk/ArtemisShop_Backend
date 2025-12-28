using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Payments.Common;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Application.Payments.Commands.HandlePaymentCallback;

public sealed class HandlePaymentCallbackCommandHandler : IRequestHandler<HandlePaymentCallbackCommand, Order?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly ILogger<HandlePaymentCallbackCommandHandler>? _logger;

    public HandlePaymentCallbackCommandHandler(
        IApplicationDbContext context,
        IEnumerable<IPaymentProvider> paymentProviders,
        IEmailService emailService,
        IUserService userService,
        ILogger<HandlePaymentCallbackCommandHandler>? logger = null)
    {
        _context = context;
        _paymentProviders = paymentProviders;
        _emailService = emailService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Order?> Handle(HandlePaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Processing payment callback for provider: {Provider}", request.Provider);

        var provider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            _logger?.LogWarning("Payment provider not found: {Provider}", request.Provider);
            return null;
        }

        var callbackResult = await provider.VerifyCallbackAsync(request.CallbackData, cancellationToken);
        if (!callbackResult.Success)
        {
            _logger?.LogWarning("Payment callback verification failed. Error: {Error}", callbackResult.ErrorMessage);
            return null;
        }

        _logger?.LogInformation("Callback verified successfully. OrderId: {OrderId}, Amount: {Amount}", 
            callbackResult.OrderId, callbackResult.Amount);

        // For PayOS, OrderId in callback result is actually the orderCode (string)
        // We need to find the order by PaymentTransactionId (which stores the orderCode)
        Order? order = null;
        
        if (request.Provider.Equals("PayOS", StringComparison.OrdinalIgnoreCase))
        {
            // PayOS returns orderCode as OrderId, find order by PaymentTransactionId
            order = await _context.Orders
                .FirstOrDefaultAsync(o => o.PaymentTransactionId == callbackResult.OrderId, cancellationToken);
            
            _logger?.LogInformation("Searching for PayOS order with PaymentTransactionId: {OrderCode}. Found: {Found}", 
                callbackResult.OrderId, order != null);
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

        if (order == null)
        {
            _logger?.LogWarning("Order not found for callback. OrderId: {OrderId}", callbackResult.OrderId);
            return null;
        }

        // Check if payment is already processed
        if (order.PaymentStatus == 1) // Already paid
        {
            _logger?.LogInformation("Order {OrderId} already has PaymentStatus = Paid. Skipping update.", order.Id);
            return order;
        }

        _logger?.LogInformation("Updating order {OrderId}. Current OrderStatus: {OrderStatus}, PaymentStatus: {PaymentStatus}", 
            order.Id, order.OrderStatus, order.PaymentStatus);

        // Update payment status to Paid
        // Note: We update PaymentStatus regardless of OrderStatus, because payment can succeed
        // even if order is in Processing or other states
        order.PaymentStatus = 1; // Paid
        
        // Only update OrderStatus to Paid if it's currently Pending
        // This allows orders that are already Processing to keep their status
        if (order.OrderStatus == (int)OrderStatus.Pending)
        {
            order.OrderStatus = (int)OrderStatus.Paid;
            _logger?.LogInformation("Updated OrderStatus from Pending to Paid");
        }
        else
        {
            _logger?.LogInformation("OrderStatus remains {OrderStatus}, only PaymentStatus updated to Paid", order.OrderStatus);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation("Order {OrderId} payment status updated successfully. OrderStatus: {OrderStatus}, PaymentStatus: {PaymentStatus}", 
            order.Id, order.OrderStatus, order.PaymentStatus);

        // Send order confirmation email after successful payment (PayOS)
        if (order.UserId.HasValue)
        {
            try
            {
                var user = await _userService.FindByIdAsync(order.UserId.Value);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    // Load order with items for email
                    var orderWithItems = await _context.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

                    if (orderWithItems != null)
                    {
                        await _emailService.SendOrderConfirmationAsync(
                            user.Email,
                            user.FullName ?? user.Email,
                            orderWithItems,
                            cancellationToken);
                        
                        _logger?.LogInformation("Order confirmation email sent to {Email} for order {OrderNumber} after payment", 
                            user.Email, order.OrderNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send order confirmation email for order {OrderNumber} after payment. Payment was processed successfully.", 
                    order.OrderNumber);
                // Don't throw - payment is processed successfully, email failure shouldn't fail the callback
            }
        }

        return order;
    }
}

