using AtermisShop.Application.Payments.Common;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AtermisShop.Infrastructure.Payments;

public class PayOsPaymentProvider : IPaymentProvider
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public string ProviderName => "PayOS";

    public PayOsPaymentProvider(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<PaymentUrlResult> CreatePaymentUrlAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _configuration["Payments:PayOS:ClientId"];
            var apiKey = _configuration["Payments:PayOS:ApiKey"];
            var checksumKey = _configuration["Payments:PayOS:ChecksumKey"];
            var returnUrl = request.ReturnUrl ?? _configuration["Payments:PayOS:ReturnUrl"];
            var cancelUrl = request.CancelUrl ?? _configuration["Payments:PayOS:CancelUrl"];

            var orderCode = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var amount = (int)request.Amount;
            var description = request.OrderDescription;

            var requestBody = new
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                items = new[]
                {
                    new { name = description, quantity = 1, price = amount }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", clientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await _httpClient.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (result.TryGetProperty("data", out var data) && data.TryGetProperty("checkoutUrl", out var checkoutUrl))
            {
                return new PaymentUrlResult(true, checkoutUrl.GetString());
            }

            return new PaymentUrlResult(false, null, "Failed to create payment URL");
        }
        catch (Exception ex)
        {
            return new PaymentUrlResult(false, null, ex.Message);
        }
    }

    public Task<PaymentCallbackResult> VerifyCallbackAsync(Dictionary<string, string> callbackData, CancellationToken cancellationToken)
    {
        // PayOS webhook verification
        // In production, verify webhook signature
        var orderCode = callbackData.GetValueOrDefault("orderCode", "");
        var amount = decimal.Parse(callbackData.GetValueOrDefault("amount", "0"));
        var description = callbackData.GetValueOrDefault("description", "");
        var accountNumber = callbackData.GetValueOrDefault("accountNumber", "");
        var reference = callbackData.GetValueOrDefault("reference", "");
        var transactionDateTime = callbackData.GetValueOrDefault("transactionDateTime", "");
        var currency = callbackData.GetValueOrDefault("currency", "");
        var paymentLinkId = callbackData.GetValueOrDefault("paymentLinkId", "");
        var code = callbackData.GetValueOrDefault("code", "");
        var desc = callbackData.GetValueOrDefault("desc", "");
        var counterAccountBankId = callbackData.GetValueOrDefault("counterAccountBankId", "");
        var counterAccountBankName = callbackData.GetValueOrDefault("counterAccountBankName", "");
        var counterAccountName = callbackData.GetValueOrDefault("counterAccountName", "");
        var counterAccountNumber = callbackData.GetValueOrDefault("counterAccountNumber", "");
        var virtualAccountName = callbackData.GetValueOrDefault("virtualAccountName", "");
        var virtualAccountNumber = callbackData.GetValueOrDefault("virtualAccountNumber", "");
        var amountPaid = callbackData.GetValueOrDefault("amountPaid", "0");
        var amountRemaining = callbackData.GetValueOrDefault("amountRemaining", "0");

        if (code == "00" && decimal.Parse(amountPaid) >= amount)
        {
            return Task.FromResult(new PaymentCallbackResult(true, orderCode, amount, reference));
        }

        return Task.FromResult(new PaymentCallbackResult(false, orderCode, amount, reference, desc));
    }
}

