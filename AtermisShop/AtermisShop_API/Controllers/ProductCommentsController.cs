using AtermisShop.Application.Products.Comments.Commands.CreateProductComment;
using AtermisShop.Application.Products.Comments.Commands.DeleteProductComment;
using AtermisShop.Application.Products.Comments.Commands.ReplyProductComment;
using AtermisShop.Application.Products.Comments.Commands.UpdateProductComment;
using AtermisShop.Application.Products.Comments.Common;
using AtermisShop.Application.Products.Comments.Queries.GetProductComments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/products/slug/{slug}/comments")]
public class ProductCommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductCommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCommentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(string slug, CancellationToken cancellationToken)
    {
        var comments = await _mediator.Send(new GetProductCommentsQuery(slug), cancellationToken);
        return Ok(comments);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductCommentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateComment(string slug, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var comment = await _mediator.Send(new CreateProductCommentCommand(
            userId, slug, request.Content, null), cancellationToken);
        return Ok(comment);
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(string slug, Guid commentId, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new UpdateProductCommentCommand(commentId, userId, request.Content), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(string slug, Guid commentId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new DeleteProductCommentCommand(commentId, userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{parentCommentId}/reply")]
    [Authorize]
    [ProducesResponseType(typeof(ProductCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplyComment(
        string slug,
        Guid parentCommentId,
        [FromBody] ReplyCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var comment = await _mediator.Send(new ReplyProductCommentCommand(
            userId, slug, parentCommentId, request.Content), cancellationToken);
        return Ok(comment);
    }

    public record CreateCommentRequest(string Content);
    public record UpdateCommentRequest(string Content);
    public record ReplyCommentRequest(string Content);
}

