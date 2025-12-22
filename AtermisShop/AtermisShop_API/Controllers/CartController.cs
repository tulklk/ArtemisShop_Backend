using AtermisShop.Application.Cart.Commands.AddCartItem;
using AtermisShop.Application.Cart.Commands.ClearCart;
using AtermisShop.Application.Cart.Commands.DeleteCartItem;
using AtermisShop.Application.Cart.Commands.UpdateCartItem;
using AtermisShop.Application.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var cart = await _mediator.Send(new GetCartQuery(userId), cancellationToken);
        return Ok(cart);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new ClearCartCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var cartId = await _mediator.Send(new AddCartItemCommand(userId, request.ProductId, request.Quantity), cancellationToken);
        return Ok(new { CartId = cartId });
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateCartItemCommand(cartItemId, request.Quantity), cancellationToken);
        return NoContent();
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> DeleteItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCartItemCommand(cartItemId), cancellationToken);
        return NoContent();
    }

    public record AddCartItemRequest(Guid ProductId, int Quantity);
    public record UpdateCartItemRequest(int Quantity);
}

