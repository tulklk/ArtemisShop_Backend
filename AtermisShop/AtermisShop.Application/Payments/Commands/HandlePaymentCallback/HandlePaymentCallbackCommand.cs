using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Payments.Commands.HandlePaymentCallback;

public sealed record HandlePaymentCallbackCommand(
    string Provider,
    Dictionary<string, string> CallbackData) : IRequest<Order?>;

