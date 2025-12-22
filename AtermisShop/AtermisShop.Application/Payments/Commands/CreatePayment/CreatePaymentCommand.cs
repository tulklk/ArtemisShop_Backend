using MediatR;

namespace AtermisShop.Application.Payments.Commands.CreatePayment;

public sealed record CreatePaymentCommand(
    string Provider,
    Guid OrderId,
    decimal Amount,
    string OrderDescription,
    string? ReturnUrl = null) : IRequest<CreatePaymentResult>;

public sealed record CreatePaymentResult(
    bool Success,
    string? PaymentUrl,
    string? ErrorMessage = null);

