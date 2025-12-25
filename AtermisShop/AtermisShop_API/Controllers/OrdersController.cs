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
            request.ShippingAddress != null ? new AtermisShop.Application.Orders.Commands.CreateOrder.ShippingAddressDto(
                request.ShippingAddress.FullName,
                request.ShippingAddress.PhoneNumber,
                request.ShippingAddress.AddressLine,
                request.ShippingAddress.Ward,
                request.ShippingAddress.District,
                request.ShippingAddress.City
            ) : null,
            request.PaymentMethod,
            request.VoucherCode), cancellationToken);
        return Ok(order);
    }

    [HttpPost("apply-voucher")]
    public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new ApplyVoucherCommand(request.Code, userId), cancellationToken);
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

    public sealed class CreateOrderRequest
    {
        public ShippingAddressDto? ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } = default!;
        public string? VoucherCode { get; set; }
    }

    public sealed class ShippingAddressDto
    {
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public string Ward { get; set; } = default!;
        public string District { get; set; } = default!;
        public string City { get; set; } = default!;
    }

    public sealed class ApplyVoucherRequest
    {
        public string Code { get; set; } = default!;
    }
    public record CreatePaymentRequest(string Provider, string? ReturnUrl);
}

