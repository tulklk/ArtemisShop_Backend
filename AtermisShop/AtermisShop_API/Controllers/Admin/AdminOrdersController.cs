using AtermisShop.Application.Orders.Queries.GetAllOrders;
using AtermisShop.Application.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement update order status command
        return Ok();
    }

    public record UpdateOrderStatusRequest(string Status);
}

