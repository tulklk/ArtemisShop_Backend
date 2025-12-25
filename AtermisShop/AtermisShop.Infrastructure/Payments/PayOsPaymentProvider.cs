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
            var clientId = _configuration["Payment:PayOS:ClientId"];
            var apiKey = _configuration["Payment:PayOS:ApiKey"];
            var checksumKey = _configuration["Payment:PayOS:ChecksumKey"];
            var returnUrl = request.ReturnUrl ?? _configuration["Payment:PayOS:ReturnUrl"];
            var cancelUrl = request.CancelUrl ?? _configuration["Payment:PayOS:CancelUrl"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
            {
                return new PaymentUrlResult(false, null, "PayOS configuration is missing. Please check appsettings.json");
            }

            // Use OrderId to generate a unique orderCode for PayOS
            // PayOS requires orderCode to be unique and between 100000 and 999999999999
            // We'll use a combination of timestamp and hash of OrderId
            var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var orderIdHash = Math.Abs(request.OrderId.GetHashCode());
            var orderCode = (timestamp % 1000000000) * 1000 + (orderIdHash % 1000);
            if (orderCode < 100000) orderCode += 100000;
            if (orderCode > 999999999999) orderCode = orderCode % 999999999999;
            
            var amount = (int)request.Amount;
            var description = request.OrderDescription;

            // Build items array from request.Items
            var items = request.Items.Select(item => new
            {
                name = item.Name,
                quantity = item.Quantity,
                price = item.Price
            }).ToArray();

            var requestBody = new
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                items = items
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
                // Store orderCode for later use in callback verification
                return new PaymentUrlResult(true, checkoutUrl.GetString(), null, orderCode.ToString());
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
        try
        {
            var checksumKey = _configuration["Payment:PayOS:ChecksumKey"];
            
            // Extract data from callback - PayOS can send data in different formats
            // For Return URL: query params like code, id, cancel, status, orderCode
            // For Webhook: JSON body with code, desc, success, data object, signature
            
            string code = "";
            string desc = "";
            string finalOrderCode = "";
            decimal amount = 0;
            string reference = "";
            
            // Check if this is webhook format (has "data" nested object)
            if (callbackData.ContainsKey("data.code") || callbackData.ContainsKey("data.orderCode"))
            {
                // Webhook format
                code = callbackData.GetValueOrDefault("data.code", callbackData.GetValueOrDefault("code", ""));
                desc = callbackData.GetValueOrDefault("data.desc", callbackData.GetValueOrDefault("desc", ""));
                finalOrderCode = callbackData.GetValueOrDefault("data.orderCode", callbackData.GetValueOrDefault("orderCode", ""));
                
                var amountStr = callbackData.GetValueOrDefault("data.amount", callbackData.GetValueOrDefault("amount", "0"));
                if (!decimal.TryParse(amountStr, out amount))
                {
                    amount = 0;
                }
                
                reference = callbackData.GetValueOrDefault("data.reference", callbackData.GetValueOrDefault("reference", ""));
            }
            else
            {
                // Return URL format (query params)
                code = callbackData.GetValueOrDefault("code", "");
                finalOrderCode = callbackData.GetValueOrDefault("orderCode", "");
                // For return URL, we need to get order amount from database using orderCode
                // For now, we'll use 0 and let the handler fetch it
                amount = 0;
                reference = callbackData.GetValueOrDefault("id", ""); // Payment Link Id
            }
            
            // Verify signature if provided (for webhook)
            var signature = callbackData.GetValueOrDefault("signature", "");
            if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(checksumKey))
            {
                // TODO: Implement signature verification using checksumKey
                // PayOS signature verification: HMAC SHA256 of sorted data
            }
            
            // PayOS success code is "00"
            // For return URL, status "PAID" also indicates success
            var status = callbackData.GetValueOrDefault("status", "");
            var isSuccess = code == "00" || status == "PAID";
            
            if (isSuccess && !string.IsNullOrEmpty(finalOrderCode))
            {
                return Task.FromResult(new PaymentCallbackResult(true, finalOrderCode, amount, reference));
            }

            return Task.FromResult(new PaymentCallbackResult(false, finalOrderCode, amount, reference, desc));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new PaymentCallbackResult(false, "", 0, "", ex.Message));
        }
    }
}

