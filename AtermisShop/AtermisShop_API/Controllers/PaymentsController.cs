using AtermisShop.Application.Payments.Commands.CreatePayment;
using AtermisShop.Application.Payments.Commands.HandlePaymentCallback;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("momo/return")]
    [AllowAnonymous]
    public async Task<IActionResult> MomoReturn([FromQuery] Dictionary<string, string> queryParams, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("Momo", queryParams), cancellationToken);
        if (result == null)
            return BadRequest(new { message = "Payment verification failed" });
        return Ok(new { message = "Payment successful", orderId = result.Id });
    }

    [HttpPost("momo/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> MomoIpn([FromForm] Dictionary<string, string> formData, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("Momo", formData), cancellationToken);
        if (result == null)
            return BadRequest();
        return Ok(new { message = "OK" });
    }

    [HttpGet("vnpay/return")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayReturn([FromQuery] Dictionary<string, string> queryParams, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("VNPay", queryParams), cancellationToken);
        if (result == null)
            return BadRequest(new { message = "Payment verification failed" });
        return Ok(new { message = "Payment successful", orderId = result.Id });
    }

    [HttpPost("vnpay/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayIpn([FromForm] Dictionary<string, string> formData, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("VNPay", formData), cancellationToken);
        if (result == null)
            return BadRequest();
        return Ok(new { message = "OK" });
    }

    [HttpGet("payos/return")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsReturn([FromQuery] Dictionary<string, string> queryParams, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("PayOS", queryParams), cancellationToken);
        if (result == null)
            return BadRequest(new { message = "Payment verification failed" });
        return Ok(new { message = "Payment successful", orderId = result.Id });
    }

    [HttpPost("payos/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsWebhook([FromBody] Dictionary<string, string> bodyData, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("PayOS", bodyData), cancellationToken);
        if (result == null)
            return BadRequest();
        return Ok(new { message = "OK" });
    }

    [HttpPost("payos/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsConfirm([FromBody] Dictionary<string, string> bodyData, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HandlePaymentCallbackCommand("PayOS", bodyData), cancellationToken);
        if (result == null)
            return BadRequest();
        return Ok(new { message = "OK" });
    }
}

