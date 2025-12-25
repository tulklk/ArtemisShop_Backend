using AtermisShop.Application.Payments.Common;
using MediatR;

namespace AtermisShop.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResult>
{
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;

    public CreatePaymentCommandHandler(IEnumerable<IPaymentProvider> paymentProviders)
    {
        _paymentProviders = paymentProviders;
    }

    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var provider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            return new CreatePaymentResult(false, null, $"Payment provider '{request.Provider}' not found");
        }

        var paymentRequest = new CreatePaymentRequest(
            request.OrderId.ToString(),
            request.Amount,
            request.OrderDescription,
            request.Items,
            request.ReturnUrl,
            request.CancelUrl);

        var result = await provider.CreatePaymentUrlAsync(paymentRequest, cancellationToken);
        return new CreatePaymentResult(result.Success, result.PaymentUrl, result.ErrorMessage, result.OrderCode);
    }
}

