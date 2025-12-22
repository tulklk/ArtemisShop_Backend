using AtermisShop.Application.Orders.Commands.ApplyVoucher;
using AtermisShop.Application.Orders.Commands.CreateOrder;
using AtermisShop.Application.Orders.Queries.GetMyOrders;
using AtermisShop.Application.Orders.Queries.GetOrderById;
using AtermisShop.Application.Payments.Commands.CreatePayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var order = await _mediator.Send(new CreateOrderCommand(
            userId,
            request.ShippingAddress,
            request.Notes,
            request.VoucherCode), cancellationToken);
        return Ok(order);
    }

    [HttpPost("apply-voucher")]
    public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApplyVoucherCommand(request.VoucherCode, request.OrderAmount), cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var orders = await _mediator.Send(new GetMyOrdersQuery(userId), cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var order = await _mediator.Send(new GetOrderByIdQuery(id, userId), cancellationToken);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetOrderStatus(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var order = await _mediator.Send(new GetOrderByIdQuery(id, userId), cancellationToken);
        if (order == null)
            return NotFound();
        return Ok(new { Status = ((AtermisShop.Domain.Orders.OrderStatus)order.OrderStatus).ToString(), OrderNumber = order.OrderNumber });
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> CreatePayment(Guid id, [FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var order = await _mediator.Send(new GetOrderByIdQuery(id, userId), cancellationToken);
        if (order == null)
            return NotFound();

        var paymentResult = await _mediator.Send(new CreatePaymentCommand(
            request.Provider,
            order.Id,
            order.TotalAmount,
            $"Order #{order.OrderNumber}",
            request.ReturnUrl), cancellationToken);

        if (!paymentResult.Success)
            return BadRequest(new { message = paymentResult.ErrorMessage });

        return Ok(new { PaymentUrl = paymentResult.PaymentUrl });
    }

    [HttpPost("{id}/test-email")]
    public async Task<IActionResult> TestEmail(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement email sending
        return Ok(new { message = "Test email endpoint - not implemented yet" });
    }

    public record CreateOrderRequest(string? ShippingAddress, string? Notes, string? VoucherCode);
    public record ApplyVoucherRequest(string VoucherCode, decimal OrderAmount);
    public record CreatePaymentRequest(string Provider, string? ReturnUrl);
}

