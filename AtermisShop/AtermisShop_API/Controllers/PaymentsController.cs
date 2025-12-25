using AtermisShop.Application.Payments.Commands.CreatePayment;
using AtermisShop.Application.Payments.Commands.HandlePaymentCallback;
using AtermisShop_API.Controllers.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
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
            return BadRequest(new { message = "Payment verification failed" });
        return Ok(new { message = "Payment successful", orderId = result.Id });
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

