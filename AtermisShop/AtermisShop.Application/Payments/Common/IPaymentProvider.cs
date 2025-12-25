namespace AtermisShop.Application.Payments.Common;

public interface IPaymentProvider
{
    Task<PaymentUrlResult> CreatePaymentUrlAsync(CreatePaymentRequest request, CancellationToken cancellationToken);
    Task<PaymentCallbackResult> VerifyCallbackAsync(Dictionary<string, string> callbackData, CancellationToken cancellationToken);
    string ProviderName { get; }
}

public record CreatePaymentRequest(
    string OrderId,
    decimal Amount,
    string OrderDescription,
    List<PaymentItem> Items,
    string? ReturnUrl = null,
    string? CancelUrl = null);

public record PaymentUrlResult(
    bool Success,
    string? PaymentUrl,
    string? ErrorMessage = null,
    string? OrderCode = null);

public record PaymentCallbackResult(
    bool Success,
    string OrderId,
    decimal Amount,
    string TransactionId,
    string? ErrorMessage = null);

