using AtermisShop.Application.Payments.Commands.CreatePayment;
using AtermisShop.Application.Payments.Commands.HandlePaymentCallback;
using AtermisShop_API.Controllers.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public PaymentsController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    private string BuildFrontendRedirectUrl(string path, Dictionary<string, string?> query)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "";
        if (string.IsNullOrWhiteSpace(frontendUrl) || !Uri.TryCreate(frontendUrl, UriKind.Absolute, out var frontendUri))
        {
            // Fall back to relative redirect; FE can handle it if hosted same domain.
            var relative = path.StartsWith('/') ? path : "/" + path;
            var q = string.Join("&", query
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
            return string.IsNullOrWhiteSpace(q) ? relative : $"{relative}?{q}";
        }

        var baseUrl = frontendUri.ToString().TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : "/" + path;
        var queryString = string.Join("&", query
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));

        return string.IsNullOrWhiteSpace(queryString)
            ? $"{baseUrl}{normalizedPath}"
            : $"{baseUrl}{normalizedPath}?{queryString}";
    }

    [HttpGet("payos/return")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsReturn([FromQuery] PayOsReturnRequest request, CancellationToken cancellationToken)
    {
        // Convert request to dictionary for backward compatibility
        var queryParams = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(request.Code)) queryParams["code"] = request.Code;
        if (!string.IsNullOrEmpty(request.Id)) queryParams["id"] = request.Id;
        if (request.Cancel.HasValue) queryParams["cancel"] = request.Cancel.Value.ToString().ToLower();
        if (!string.IsNullOrEmpty(request.Status)) queryParams["status"] = request.Status;
        if (request.OrderCode.HasValue) queryParams["orderCode"] = request.OrderCode.Value.ToString();

        var result = await _mediator.Send(new HandlePaymentCallbackCommand("PayOS", queryParams), cancellationToken);
        if (result == null)
        {
            var failUrl = BuildFrontendRedirectUrl("/payment/cancel", new Dictionary<string, string?>
            {
                ["provider"] = "PayOS",
                ["status"] = request.Status,
                ["orderCode"] = request.OrderCode?.ToString()
            });
            return Redirect(failUrl);
        }

        var successUrl = BuildFrontendRedirectUrl("/payment/success", new Dictionary<string, string?>
        {
            ["provider"] = "PayOS",
            ["orderId"] = result.Id.ToString(),
            ["orderNumber"] = result.OrderNumber,
            ["orderCode"] = request.OrderCode?.ToString()
        });
        return Redirect(successUrl);
    }

    [HttpGet("payos/cancel")]
    [AllowAnonymous]
    public IActionResult PayOsCancel([FromQuery] PayOsReturnRequest request)
    {
        var cancelUrl = BuildFrontendRedirectUrl("/payment/cancel", new Dictionary<string, string?>
        {
            ["provider"] = "PayOS",
            ["status"] = request.Status,
            ["orderCode"] = request.OrderCode?.ToString(),
            ["cancel"] = request.Cancel?.ToString()
        });
        return Redirect(cancelUrl);
    }

    [HttpPost("payos/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsWebhook([FromBody] PayOsWebhookRequest request, CancellationToken cancellationToken)
    {
        // Convert request to dictionary for backward compatibility
        var bodyData = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(request.Code)) bodyData["code"] = request.Code;
        if (!string.IsNullOrEmpty(request.Desc)) bodyData["desc"] = request.Desc;
        bodyData["success"] = request.Success.ToString().ToLower();
        if (!string.IsNullOrEmpty(request.Signature)) bodyData["signature"] = request.Signature;
        
        if (request.Data != null)
        {
            bodyData["orderCode"] = request.Data.OrderCode.ToString();
            bodyData["amount"] = request.Data.Amount.ToString();
            if (!string.IsNullOrEmpty(request.Data.Description)) bodyData["description"] = request.Data.Description;
            if (!string.IsNullOrEmpty(request.Data.AccountNumber)) bodyData["accountNumber"] = request.Data.AccountNumber;
            if (!string.IsNullOrEmpty(request.Data.Reference)) bodyData["reference"] = request.Data.Reference;
            if (!string.IsNullOrEmpty(request.Data.TransactionDateTime)) bodyData["transactionDateTime"] = request.Data.TransactionDateTime;
            if (!string.IsNullOrEmpty(request.Data.Currency)) bodyData["currency"] = request.Data.Currency;
            if (!string.IsNullOrEmpty(request.Data.PaymentLinkId)) bodyData["paymentLinkId"] = request.Data.PaymentLinkId;
            if (!string.IsNullOrEmpty(request.Data.Code)) bodyData["data.code"] = request.Data.Code;
            if (!string.IsNullOrEmpty(request.Data.Desc)) bodyData["data.desc"] = request.Data.Desc;
            if (!string.IsNullOrEmpty(request.Data.CounterAccountBankId)) bodyData["counterAccountBankId"] = request.Data.CounterAccountBankId;
            if (!string.IsNullOrEmpty(request.Data.CounterAccountBankName)) bodyData["counterAccountBankName"] = request.Data.CounterAccountBankName;
            if (!string.IsNullOrEmpty(request.Data.CounterAccountName)) bodyData["counterAccountName"] = request.Data.CounterAccountName;
            if (!string.IsNullOrEmpty(request.Data.CounterAccountNumber)) bodyData["counterAccountNumber"] = request.Data.CounterAccountNumber;
            if (!string.IsNullOrEmpty(request.Data.VirtualAccountName)) bodyData["virtualAccountName"] = request.Data.VirtualAccountName;
            if (!string.IsNullOrEmpty(request.Data.VirtualAccountNumber)) bodyData["virtualAccountNumber"] = request.Data.VirtualAccountNumber;
        }

        var result = await _mediator.Send(new HandlePaymentCallbackCommand("PayOS", bodyData), cancellationToken);
        if (result == null)
            return BadRequest();
        return Ok(new { message = "OK" });
    }

    [HttpPost("payos/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsConfirm([FromBody] PayOsConfirmRequest request, CancellationToken cancellationToken)
    {
        // Convert request to dictionary for backward compatibility
        var bodyData = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(request.Code)) bodyData["code"] = request.Code;
        if (!string.IsNullOrEmpty(request.Id)) bodyData["id"] = request.Id;
        bodyData["cancel"] = request.Cancel.ToString().ToLower();
        if (!string.IsNullOrEmpty(request.Status)) bodyData["status"] = request.Status;
        bodyData["orderCode"] = request.OrderCode.ToString();
        bodyData["orderId"] = request.OrderId.ToString();

        var result = await _mediator.Send(new HandlePaymentCallbackCommand("PayOS", bodyData), cancellationToken);
        if (result == null)
            return BadRequest();
        return Ok(new { message = "OK" });
    }
}

