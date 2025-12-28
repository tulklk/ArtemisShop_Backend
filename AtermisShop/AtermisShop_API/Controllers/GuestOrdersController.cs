using AtermisShop.Application.Orders.Commands.ApplyVoucher;
using AtermisShop.Application.Orders.Commands.CreateGuestOrder;
using AtermisShop.Application.Orders.Commands.CreateOrder;
using AtermisShop.Application.Orders.Common;
using AtermisShop.Application.Orders.Queries.GetOrderById;
using AtermisShop.Application.Orders.Queries.LookupGuestOrder;
using AtermisShop.Application.Payments.Commands.CreatePayment;
using AtermisShop.Application.Payments.Common;
using AtermisShop_API.Controllers.GuestOrders;
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

    /// <summary>
    /// Creates a new guest order
    /// </summary>
    /// <param name="request">Guest order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order</returns>
    /// <response code="200">Order created successfully</response>
    /// <response code="400">Bad request - Invalid input data or validation error</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateGuestOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponseDto { Message = "Request body is required." });
            }

            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(new ErrorResponseDto { Message = "Order must contain at least one item." });
            }

            var orderItems = request.Items.Select(item => new AtermisShop.Application.Orders.Commands.CreateGuestOrder.GuestOrderItem(
                item.ProductId, 
                item.ProductVariantId, 
                item.Quantity)).ToList();
            
            var shippingAddressDto = request.ShippingAddress != null 
                ? new ShippingAddressDto(
                    request.ShippingAddress.FullName,
                    request.ShippingAddress.PhoneNumber,
                    request.ShippingAddress.AddressLine,
                    request.ShippingAddress.District,
                    request.ShippingAddress.City)
                : null;

            var order = await _mediator.Send(new CreateGuestOrderCommand(
                request.Email,
                request.FullName,
                shippingAddressDto,
                request.PaymentMethod,
                orderItems,
                request.VoucherCode), cancellationToken);
            
            // Map Order entity to OrderResponseDto
            var orderResponse = new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                GuestEmail = order.GuestEmail,
                GuestFullName = order.GuestFullName,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentTransactionId = order.PaymentTransactionId,
                ShippingFullName = order.ShippingFullName,
                ShippingPhoneNumber = order.ShippingPhoneNumber,
                ShippingAddressLine = order.ShippingAddressLine,
                ShippingWard = order.ShippingWard,
                ShippingDistrict = order.ShippingDistrict,
                ShippingCity = order.ShippingCity,
                VoucherId = order.VoucherId,
                VoucherDiscountAmount = order.VoucherDiscountAmount,
                Items = order.Items.Select(item => new OrderItemResponseDto
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductNameSnapshot = item.ProductNameSnapshot,
                    VariantInfoSnapshot = item.VariantInfoSnapshot,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
            
            return Ok(orderResponse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponseDto { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponseDto { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponseDto 
            { 
                Message = "An error occurred while creating the order.", 
                Error = ex.Message 
            });
        }
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

    /// <summary>
    /// Get order status by order number for guest
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order status information</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    [HttpGet("status/{orderNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrderStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderStatus(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new LookupGuestOrderQuery(orderNumber), cancellationToken);
        if (order == null)
            return NotFound();

        var response = new OrderStatusResponseDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderStatus = OrderStatusHelper.FromInt(order.OrderStatus),
            PaymentStatus = PaymentStatusHelper.FromInt(order.PaymentStatus),
            PaymentMethod = PaymentMethod.FromInt(order.PaymentMethod),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt ?? order.CreatedAt,
            StatusDescription = OrderStatusHelper.FromInt(order.OrderStatus)
        };

        return Ok(response);
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
        string Email,
        string FullName,
        GuestShippingAddressDto? ShippingAddress,
        string PaymentMethod,
        List<GuestOrderItem> Items,
        string? VoucherCode);

    public record GuestShippingAddressDto(
        string FullName,
        string PhoneNumber,
        string AddressLine,
        string District,
        string City);

    public record GuestOrderItem(
        Guid ProductId,
        Guid? ProductVariantId,
        int Quantity);
    public sealed class ApplyVoucherRequest
    {
        public string Code { get; set; } = default!;
        public decimal OrderAmount { get; set; }
    }
    public record CreatePaymentRequest(string Provider, string? ReturnUrl = null, string? CancelUrl = null);
}

