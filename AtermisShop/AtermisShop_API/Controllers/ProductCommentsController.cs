using AtermisShop.Application.Products.Comments.Commands.CreateProductComment;
using AtermisShop.Application.Products.Comments.Commands.DeleteProductComment;
using AtermisShop.Application.Products.Comments.Commands.UpdateProductComment;
using AtermisShop.Application.Products.Comments.Queries.GetProductComments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/products/{productIdOrSlug}/comments")]
public class ProductCommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductCommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(string productIdOrSlug, CancellationToken cancellationToken)
    {
        var comments = await _mediator.Send(new GetProductCommentsQuery(productIdOrSlug), cancellationToken);
        return Ok(comments);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment(string productIdOrSlug, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var commentId = await _mediator.Send(new CreateProductCommentCommand(
            userId, productIdOrSlug, request.Content, request.ParentCommentId), cancellationToken);
        return Ok(new { CommentId = commentId });
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(string productIdOrSlug, Guid commentId, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new UpdateProductCommentCommand(commentId, userId, request.Content), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(string productIdOrSlug, Guid commentId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _mediator.Send(new DeleteProductCommentCommand(commentId, userId), cancellationToken);
        return NoContent();
    }

    public record CreateCommentRequest(string Content, Guid? ParentCommentId);
    public record UpdateCommentRequest(string Content);
}

