using MediatR;

namespace AtermisShop.Application.Orders.Commands.UpdateOrderPaymentTransaction;

public sealed record UpdateOrderPaymentTransactionCommand(
    Guid OrderId,
    string PaymentTransactionId) : IRequest<bool>;

