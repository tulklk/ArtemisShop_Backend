using AtermisShop.Application.Payments.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AtermisShop.Infrastructure.Payments;

public class PayOsPaymentProvider : IPaymentProvider
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PayOsPaymentProvider>? _logger;

    public string ProviderName => "PayOS";

    public PayOsPaymentProvider(IConfiguration configuration, HttpClient httpClient, ILogger<PayOsPaymentProvider>? logger = null)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
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

            // Validate URLs
            if (string.IsNullOrEmpty(returnUrl) || !Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
            {
                return new PaymentUrlResult(false, null, "ReturnUrl is invalid or missing");
            }

            if (string.IsNullOrEmpty(cancelUrl) || !Uri.TryCreate(cancelUrl, UriKind.Absolute, out _))
            {
                return new PaymentUrlResult(false, null, "CancelUrl is invalid or missing");
            }

            // Validate items
            if (request.Items == null || !request.Items.Any())
            {
                return new PaymentUrlResult(false, null, "Payment items cannot be empty");
            }

            // Validate and sanitize items
            var validItems = new List<object>();
            long totalAmount = 0;
            
            foreach (var item in request.Items)
            {
                // Validate item name
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    return new PaymentUrlResult(false, null, "Item name cannot be empty");
                }

                // PayOS has a limit on item name length (typically 127 characters)
                var itemName = item.Name.Length > 127 ? item.Name.Substring(0, 127) : item.Name;

                // Validate quantity
                if (item.Quantity <= 0)
                {
                    return new PaymentUrlResult(false, null, $"Item quantity must be greater than 0. Item: {itemName}");
                }

                // Validate price
                if (item.Price <= 0)
                {
                    return new PaymentUrlResult(false, null, $"Item price must be greater than 0. Item: {itemName}");
                }

                // Calculate line total
                var lineTotal = (long)item.Price * item.Quantity;
                totalAmount += lineTotal;

                validItems.Add(new
                {
                    name = itemName,
                    quantity = item.Quantity,
                    price = item.Price
                });
            }

            // Generate orderCode: PayOS requires orderCode to be unique and between 100000 and 999999999999
            // Use timestamp (last 9 digits) + random component to ensure uniqueness
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var randomComponent = RandomNumberGenerator.GetInt32(1000, 9999); // 4-digit random
            var orderCode = (timestamp % 1000000000) * 10000 + randomComponent;
            
            // Ensure orderCode is within valid range (100000 to 999999999999)
            if (orderCode < 100000)
            {
                orderCode += 100000;
            }
            else if (orderCode > 999999999999)
            {
                // If too large, use modulo to bring it within range
                orderCode = (orderCode % 999999999999);
                if (orderCode < 100000)
                {
                    orderCode += 100000;
                }
            }

            // Use calculated total amount
            var calculatedAmount = totalAmount;
            
            // Validate calculated amount
            if (calculatedAmount <= 0)
            {
                return new PaymentUrlResult(false, null, "Total payment amount must be greater than 0");
            }

            // Ensure amount doesn't exceed PayOS limit (typically 500,000,000 VND)
            if (calculatedAmount > 500000000)
            {
                return new PaymentUrlResult(false, null, "Payment amount exceeds maximum limit (500,000,000 VND)");
            }

            // Sanitize description (PayOS may have length limits)
            var description = request.OrderDescription ?? $"Order {request.OrderId}";
            if (description.Length > 255)
            {
                description = description.Substring(0, 255);
            }

            // Calculate signature for PayOS
            // Signature is calculated from: amount, orderCode, description, returnUrl, cancelUrl
            // PayOS requires keys to be in alphabetical order: amount, cancelUrl, description, orderCode, returnUrl
            // Using HMAC SHA256 with checksumKey
            // IMPORTANT: PayOS requires query string format, NOT JSON format
            // Format: amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl
            
            // Build query string in alphabetical order
            var signatureData = $"amount={calculatedAmount}&cancelUrl={Uri.EscapeDataString(cancelUrl)}&description={Uri.EscapeDataString(description)}&orderCode={orderCode}&returnUrl={Uri.EscapeDataString(returnUrl)}";

            // Calculate HMAC SHA256 signature
            var signature = CalculateHMACSHA256(signatureData, checksumKey);
            
            // Log detailed signature calculation info for debugging
            _logger?.LogInformation("=== PayOS Signature Calculation ===");
            _logger?.LogInformation("Amount: {Amount}", calculatedAmount);
            _logger?.LogInformation("OrderCode: {OrderCode}", orderCode);
            _logger?.LogInformation("Description: {Description}", description);
            _logger?.LogInformation("ReturnUrl: {ReturnUrl}", returnUrl);
            _logger?.LogInformation("CancelUrl: {CancelUrl}", cancelUrl);
            _logger?.LogInformation("Signature Query String: {SignatureData}", signatureData);
            _logger?.LogInformation("Calculated Signature: {Signature}", signature);
            _logger?.LogInformation("ChecksumKey Length: {KeyLength}", checksumKey?.Length ?? 0);
            _logger?.LogInformation("ChecksumKey (first 10 chars): {KeyPreview}", 
                checksumKey?.Length > 10 ? checksumKey.Substring(0, 10) + "..." : checksumKey);

            var requestBody = new
            {
                orderCode = orderCode,
                amount = (int)calculatedAmount,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                items = validItems,
                signature = signature
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger?.LogInformation("PayOS Payment Request - OrderCode: {OrderCode}, Amount: {Amount}, Items: {ItemCount}", 
                orderCode, calculatedAmount, validItems.Count);
            _logger?.LogDebug("PayOS Request Body: {RequestBody}", json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-client-id", clientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await _httpClient.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger?.LogInformation("PayOS API Response - Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentUrlResult(false, null, $"PayOS API returned status {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Check for error response from PayOS
            if (result.TryGetProperty("code", out var codeElement))
            {
                var code = codeElement.GetString();
                if (code != "00")
                {
                    var desc = result.TryGetProperty("desc", out var descElement) 
                        ? descElement.GetString() 
                        : "Unknown error";
                    
                    _logger?.LogError("PayOS Error - Code: {Code}, Description: {Desc}, Request: {Request}", 
                        code, desc, json);
                    
                    return new PaymentUrlResult(false, null, $"PayOS error: {desc} (Code: {code})");
                }
            }

            // Check if data exists and is not null
            if (result.TryGetProperty("data", out var data))
            {
                // Check if data is null
                if (data.ValueKind == JsonValueKind.Null)
                {
                    return new PaymentUrlResult(false, null, "PayOS returned null data. Please check your PayOS configuration and order details.");
                }

                // Check if data is an object and has checkoutUrl
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("checkoutUrl", out var checkoutUrl))
                {
                    var paymentUrl = checkoutUrl.GetString();
                    _logger?.LogInformation("PayOS Payment URL created successfully: {PaymentUrl}", paymentUrl);
                    
                    // Store orderCode for later use in callback verification
                    return new PaymentUrlResult(true, paymentUrl, null, orderCode.ToString());
                }
            }

            return new PaymentUrlResult(false, null, $"Failed to create payment URL. Response: {responseContent}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating PayOS payment URL");
            return new PaymentUrlResult(false, null, $"Error: {ex.Message}");
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

    /// <summary>
    /// Calculate HMAC SHA256 signature for PayOS
    /// </summary>
    private string CalculateHMACSHA256(string data, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}

