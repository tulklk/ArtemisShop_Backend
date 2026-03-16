using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Orders.Commands.ApplyVoucher;
using AtermisShop.Application.Orders.Commands.CreateOrder;
using AtermisShop.Application.Orders.Commands.UpdateOrderPaymentTransaction;
using AtermisShop.Application.Orders.Common;
using AtermisShop.Application.Orders.Queries.GetMyOrders;
using AtermisShop.Application.Orders.Queries.GetOrderById;
using AtermisShop.Application.Payments.Commands.CreatePayment;
using AtermisShop.Application.Payments.Common;
using AtermisShop_API.Controllers.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public OrdersController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var order = await _mediator.Send(new CreateOrderCommand(
                userId,
                request.ShippingAddress != null ? new AtermisShop.Application.Orders.Commands.CreateOrder.ShippingAddressDto(
                    request.ShippingAddress.FullName,
                    request.ShippingAddress.PhoneNumber,
                    request.ShippingAddress.AddressLine,
                    request.ShippingAddress.District,
                    request.ShippingAddress.City
                ) : null,
                request.PaymentMethod,
                request.VoucherCode), cancellationToken);
            
            // Map Order entity to OrderDto to avoid circular reference
            var orderDto = await _mediator.Send(new GetOrderByIdQuery(order.Id, userId), cancellationToken);
            return Ok(orderDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, statusCode = 400 });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, statusCode = 400 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the order.", error = ex.Message, statusCode = 500 });
        }
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

        // Convert OrderItems to PaymentItems for PayOS
        var paymentItems = order.Items.Select(item => new PaymentItem(
            Name: item.ProductNameSnapshot + (!string.IsNullOrEmpty(item.VariantInfoSnapshot) ? $" - {item.VariantInfoSnapshot}" : ""),
            Quantity: item.Quantity,
            Price: (int)Math.Round(item.UnitPrice, MidpointRounding.AwayFromZero) // Convert decimal to int (VND) with proper rounding
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

        // For PayOS, save orderCode to PaymentTransactionId for callback verification
        if (request.Provider.Equals("PayOS", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(paymentResult.OrderCode))
        {
            await _mediator.Send(new UpdateOrderPaymentTransactionCommand(
                order.Id, paymentResult.OrderCode), cancellationToken);
        }

        // Build response according to API specification
        var response = new PaymentResponseDto
        {
            PaymentUrl = paymentResult.PaymentUrl ?? string.Empty,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            IsCod = order.PaymentMethod == 0, // 0 = COD
            IsBankTransfer = false, // PayOS is not bank transfer, it's online payment
            BankTransferInfo = null // Only set if payment method is bank transfer
        };

        return Ok(response);
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
        public string District { get; set; } = default!;
        public string City { get; set; } = default!;
    }

    public sealed class ApplyVoucherRequest
    {
        public string Code { get; set; } = default!;
    }
    public record CreatePaymentRequest(string Provider, string? ReturnUrl = null, string? CancelUrl = null);

    [HttpGet("debug-voucher/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugVoucher(string code, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);

        if (voucher == null)
        {
            var allCodes = await _context.Vouchers.Select(v => v.Code).ToListAsync(cancellationToken);
            return Ok(new
            {
                found = false,
                serverTimeUtc = now.ToString("o"),
                message = $"Voucher with code '{code}' not found in database",
                availableCodes = allCodes
            });
        }

        var checks = new Dictionary<string, object>
        {
            ["1_voucherFound"] = true,
            ["2_startDate"] = voucher.StartDate.ToString("o"),
            ["3_endDate"] = voucher.EndDate.ToString("o"),
            ["4_serverTimeUtc"] = now.ToString("o"),
            ["5_startDatePassed"] = voucher.StartDate <= now,
            ["6_endDateNotPassed"] = voucher.EndDate >= now,
            ["7_isWithinDateRange"] = voucher.StartDate <= now && voucher.EndDate >= now,
            ["8_usageLimitTotal"] = voucher.UsageLimitTotal,
            ["9_usedCount"] = voucher.UsedCount,
            ["10_usageLimitOk"] = voucher.UsageLimitTotal <= 0 || voucher.UsedCount < voucher.UsageLimitTotal,
            ["11_isPublic"] = voucher.IsPublic,
            ["12_discountType"] = voucher.DiscountType == 0 ? "FixedAmount" : "Percent",
            ["13_discountValue"] = voucher.DiscountValue,
            ["14_maxDiscountAmount"] = voucher.MaxDiscountAmount,
            ["15_minOrderAmount"] = voucher.MinOrderAmount
        };

        var failReasons = new List<string>();
        if (voucher.StartDate > now)
            failReasons.Add($"Voucher chua bat dau. StartDate={voucher.StartDate:o}, ServerUTC={now:o}");
        if (voucher.EndDate < now)
            failReasons.Add($"Voucher da het han. EndDate={voucher.EndDate:o}, ServerUTC={now:o}");
        if (voucher.UsageLimitTotal > 0 && voucher.UsedCount >= voucher.UsageLimitTotal)
            failReasons.Add($"Vuot gioi han su dung. UsedCount={voucher.UsedCount}, Limit={voucher.UsageLimitTotal}");

        return Ok(new
        {
            found = true,
            code = voucher.Code,
            name = voucher.Name,
            checks,
            failReasons = failReasons.Count > 0 ? failReasons : new List<string> { "No issues found - voucher should work" },
            hint = "If failReasons is empty but voucher still fails, check orderAmount (cart might be empty or UnitPriceSnapshot=0)"
        });
    }
}

