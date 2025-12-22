using AtermisShop.Application.Products.Reviews.Commands.CreateProductReview;
using AtermisShop.Application.Products.Reviews.Queries.GetProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/products/{productIdOrSlug}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(string productIdOrSlug, CancellationToken cancellationToken)
    {
        var reviews = await _mediator.Send(new GetProductReviewsQuery(productIdOrSlug), cancellationToken);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview(string productIdOrSlug, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var reviewId = await _mediator.Send(new CreateProductReviewCommand(
            userId, productIdOrSlug, request.Rating, request.Comment), cancellationToken);
        return Ok(new { ReviewId = reviewId });
    }

    public record CreateReviewRequest(int Rating, string Comment);
}

