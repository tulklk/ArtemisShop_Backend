using AtermisShop.Application.Payments.Common;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace AtermisShop.Infrastructure.Payments;

public class VnPayPaymentProvider : IPaymentProvider
{
    private readonly IConfiguration _configuration;

    public string ProviderName => "VNPay";

    public VnPayPaymentProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<PaymentUrlResult> CreatePaymentUrlAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tmnCode = _configuration["Payments:VNPay:TmnCode"];
            var hashSecret = _configuration["Payments:VNPay:HashSecret"];
            var returnUrl = request.ReturnUrl ?? _configuration["Payments:VNPay:ReturnUrl"];
            var ipnUrl = _configuration["Payments:VNPay:IpnUrl"];
            var url = _configuration["Payments:VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

            var vnp_Params = new SortedDictionary<string, string>();
            vnp_Params.Add("vnp_Version", "2.1.0");
            vnp_Params.Add("vnp_Command", "pay");
            vnp_Params.Add("vnp_TmnCode", tmnCode!);
            vnp_Params.Add("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            vnp_Params.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp_Params.Add("vnp_CurrCode", "VND");
            vnp_Params.Add("vnp_IpAddr", "127.0.0.1");
            vnp_Params.Add("vnp_Locale", "vn");
            vnp_Params.Add("vnp_OrderInfo", request.OrderDescription);
            vnp_Params.Add("vnp_OrderType", "other");
            vnp_Params.Add("vnp_ReturnUrl", returnUrl!);
            vnp_Params.Add("vnp_TxnRef", request.OrderId);

            var queryString = string.Join("&", vnp_Params.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            var signData = queryString;
            var vnp_SecureHash = HmacSHA512(hashSecret!, signData);
            queryString += $"&vnp_SecureHash={vnp_SecureHash}";

            var paymentUrl = $"{url}?{queryString}";
            return Task.FromResult(new PaymentUrlResult(true, paymentUrl));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new PaymentUrlResult(false, null, ex.Message));
        }
    }

    public Task<PaymentCallbackResult> VerifyCallbackAsync(Dictionary<string, string> callbackData, CancellationToken cancellationToken)
    {
        var hashSecret = _configuration["Payments:VNPay:HashSecret"];
        var vnp_SecureHash = callbackData.GetValueOrDefault("vnp_SecureHash", "");
        
        var vnp_Params = new SortedDictionary<string, string>(callbackData);
        vnp_Params.Remove("vnp_SecureHash");
        vnp_Params.Remove("vnp_SecureHashType");

        var signData = string.Join("&", vnp_Params.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var checkSum = HmacSHA512(hashSecret!, signData);

        if (checkSum != vnp_SecureHash)
        {
            return Task.FromResult(new PaymentCallbackResult(false, callbackData.GetValueOrDefault("vnp_TxnRef", ""), 0, callbackData.GetValueOrDefault("vnp_TransactionNo", ""), "Invalid signature"));
        }

        var responseCode = callbackData.GetValueOrDefault("vnp_ResponseCode", "");
        if (responseCode != "00")
        {
            return Task.FromResult(new PaymentCallbackResult(false, callbackData.GetValueOrDefault("vnp_TxnRef", ""), 0, callbackData.GetValueOrDefault("vnp_TransactionNo", ""), "Payment failed"));
        }

        var amount = decimal.Parse(callbackData.GetValueOrDefault("vnp_Amount", "0")) / 100;
        var orderId = callbackData.GetValueOrDefault("vnp_TxnRef", "");
        var transactionId = callbackData.GetValueOrDefault("vnp_TransactionNo", "");

        return Task.FromResult(new PaymentCallbackResult(true, orderId, amount, transactionId));
    }

    private static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        return string.Join("", hashValue.Select(b => b.ToString("x2")));
    }
}

