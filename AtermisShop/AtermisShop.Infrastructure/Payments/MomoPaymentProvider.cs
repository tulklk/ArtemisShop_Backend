using AtermisShop.Application.Payments.Common;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AtermisShop.Infrastructure.Payments;

public class MomoPaymentProvider : IPaymentProvider
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public string ProviderName => "Momo";

    public MomoPaymentProvider(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<PaymentUrlResult> CreatePaymentUrlAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var partnerCode = _configuration["Payments:Momo:PartnerCode"];
            var accessKey = _configuration["Payments:Momo:AccessKey"];
            var secretKey = _configuration["Payments:Momo:SecretKey"];
            var returnUrl = request.ReturnUrl ?? _configuration["Payments:Momo:ReturnUrl"];
            var notifyUrl = _configuration["Payments:Momo:IpnUrl"];
            var orderId = Guid.NewGuid().ToString();
            var amount = (long)request.Amount;
            var orderInfo = request.OrderDescription;
            var requestId = Guid.NewGuid().ToString();
            var extraData = "";

            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";
            var signature = ComputeHmacSha256(rawHash, secretKey!);

            var requestBody = new
            {
                partnerCode = partnerCode,
                partnerName = "Artemis Shop",
                storeId = "ArtemisShop",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                extraData = extraData,
                requestType = "captureWallet",
                signature = signature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (result.TryGetProperty("payUrl", out var payUrl))
            {
                return new PaymentUrlResult(true, payUrl.GetString());
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
        // Verify Momo callback signature
        var secretKey = _configuration["Payments:Momo:SecretKey"];
        var amount = callbackData.GetValueOrDefault("amount", "0");
        var orderId = callbackData.GetValueOrDefault("orderId", "");
        var partnerCode = callbackData.GetValueOrDefault("partnerCode", "");
        var accessKey = callbackData.GetValueOrDefault("accessKey", "");
        var orderInfo = callbackData.GetValueOrDefault("orderInfo", "");
        var orderType = callbackData.GetValueOrDefault("orderType", "");
        var transId = callbackData.GetValueOrDefault("transId", "");
        var resultCode = callbackData.GetValueOrDefault("resultCode", "");
        var message = callbackData.GetValueOrDefault("message", "");
        var payType = callbackData.GetValueOrDefault("payType", "");
        var responseTime = callbackData.GetValueOrDefault("responseTime", "");
        var extraData = callbackData.GetValueOrDefault("extraData", "");
        var signature = callbackData.GetValueOrDefault("signature", "");

        var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
        var computedSignature = ComputeHmacSha256(rawHash, secretKey!);

        if (computedSignature != signature || resultCode != "0")
        {
            return Task.FromResult(new PaymentCallbackResult(false, orderId, decimal.Parse(amount), transId, "Invalid signature or payment failed"));
        }

        return Task.FromResult(new PaymentCallbackResult(true, orderId, decimal.Parse(amount), transId));
    }

    private static string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

