using AtermisShop.Application.Orders.Commands.ApplyVoucher;
using AtermisShop.Application.Orders.Commands.CreateGuestOrder;
using AtermisShop.Application.Orders.Queries.GetOrderById;
using AtermisShop.Application.Orders.Queries.LookupGuestOrder;
using AtermisShop.Application.Payments.Commands.CreatePayment;
using AtermisShop.Application.Payments.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/guest/orders")]
public class GuestOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public GuestOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateOrder([FromBody] CreateGuestOrderRequest request, CancellationToken cancellationToken)
    {
        var orderItems = request.Items.Select(item => new AtermisShop.Application.Orders.Commands.CreateGuestOrder.GuestOrderItem(item.ProductId, item.Quantity)).ToList();
        var order = await _mediator.Send(new CreateGuestOrderCommand(
            request.GuestEmail,
            request.GuestPhone,
            request.GuestName,
            request.ShippingAddress,
            request.Notes,
            orderItems,
            request.VoucherCode), cancellationToken);
        return Ok(order);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (order == null || order.UserId.HasValue)
            return NotFound();
        return Ok(order);
    }

    [HttpGet("lookup")]
    [AllowAnonymous]
    public async Task<IActionResult> LookupOrder([FromQuery] string orderNumber, [FromQuery] string? email, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new LookupGuestOrderQuery(orderNumber, email), cancellationToken);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpGet("status/{orderNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrderStatus(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new LookupGuestOrderQuery(orderNumber), cancellationToken);
        if (order == null)
            return NotFound();
        return Ok(new { Status = ((AtermisShop.Domain.Orders.OrderStatus)order.OrderStatus).ToString(), OrderNumber = order.OrderNumber });
    }

    [HttpPost("apply-voucher")]
    [AllowAnonymous]
    public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApplyVoucherCommand(request.Code, null, request.OrderAmount), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/payment")]
    [AllowAnonymous]
    public async Task<IActionResult> CreatePayment(Guid id, [FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (order == null || order.UserId.HasValue)
            return NotFound();

        // Convert OrderItems to PaymentItems for PayOS
        var paymentItems = order.Items.Select(item => new PaymentItem(
            Name: item.ProductNameSnapshot + (!string.IsNullOrEmpty(item.VariantInfoSnapshot) ? $" - {item.VariantInfoSnapshot}" : ""),
            Quantity: item.Quantity,
            Price: (int)item.UnitPrice // Convert decimal to int (VND)
        )).ToList();

        var paymentResult = await _mediator.Send(new CreatePaymentCommand(
            request.Provider,
            order.Id,
            order.TotalAmount,
            $"Order #{order.OrderNumber}",
            paymentItems,
            request.ReturnUrl,
            request.CancelUrl), cancellationToken);

        if (!paymentResult.Success)
            return BadRequest(new { message = paymentResult.ErrorMessage });

        return Ok(new { PaymentUrl = paymentResult.PaymentUrl });
    }

    public record CreateGuestOrderRequest(
        string GuestEmail,
        string GuestPhone,
        string GuestName,
        string? ShippingAddress,
        string? Notes,
        List<GuestOrderItem> Items,
        string? VoucherCode);

    public record GuestOrderItem(Guid ProductId, int Quantity);
    public sealed class ApplyVoucherRequest
    {
        public string Code { get; set; } = default!;
        public decimal OrderAmount { get; set; }
    }
    public record CreatePaymentRequest(string Provider, string? ReturnUrl = null, string? CancelUrl = null);
}

