using AtermisShop.Application.Wishlist.Commands.AddToWishlist;
using AtermisShop.Application.Wishlist.Commands.RemoveFromWishlist;
using AtermisShop.Application.Wishlist.Common;
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

    /// <summary>
    /// Get user's wishlist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wishlist items</returns>
    /// <response code="200">Returns the wishlist items</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WishlistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var items = await _mediator.Send(new GetWishlistQuery(userId), cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Add product to wishlist
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wishlist item</returns>
    /// <response code="200">Product added to wishlist</response>
    /// <response code="400">Product not found or invalid</response>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(WishlistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToWishlist(Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var wishlistItem = await _mediator.Send(new AddToWishlistCommand(userId, productId), cancellationToken);
            return Ok(wishlistItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Product removed from wishlist</response>
    [HttpDelete("{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new RemoveFromWishlistCommand(userId, productId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Check if product is in wishlist
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Check result</returns>
    /// <response code="200">Returns check result</response>
    [HttpGet("{productId}/check")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var isInWishlist = await _mediator.Send(new CheckWishlistQuery(userId, productId), cancellationToken);
        return Ok(new { IsInWishlist = isInWishlist });
    }
}

