using AtermisShop.Application.Wishlist.Commands.AddToWishlist;
using AtermisShop.Application.Wishlist.Commands.RemoveFromWishlist;
using AtermisShop.Application.Wishlist.Queries.CheckWishlist;
using AtermisShop.Application.Wishlist.Queries.GetWishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var items = await _mediator.Send(new GetWishlistQuery(userId), cancellationToken);
        return Ok(items);
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new AddToWishlistCommand(userId, productId), cancellationToken);
        return Ok(new { message = "Added to wishlist" });
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new RemoveFromWishlistCommand(userId, productId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{productId}/check")]
    public async Task<IActionResult> CheckWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var isInWishlist = await _mediator.Send(new CheckWishlistQuery(userId, productId), cancellationToken);
        return Ok(new { IsInWishlist = isInWishlist });
    }
}

