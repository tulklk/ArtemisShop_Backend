using AtermisShop.Application.Products.Reviews.Commands.CreateProductReview;
using AtermisShop.Application.Products.Reviews.Common;
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
    [ProducesResponseType(typeof(IReadOnlyList<ProductReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(string productIdOrSlug, CancellationToken cancellationToken)
    {
        var reviews = await _mediator.Send(new GetProductReviewsQuery(productIdOrSlug), cancellationToken);
        return Ok(reviews);
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateReview(string productIdOrSlug, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        var review = await _mediator.Send(new CreateProductReviewCommand(
            userId, 
            productIdOrSlug, 
            request.FullName, 
            request.PhoneNumber, 
            request.Email, 
            request.Rating, 
            request.Comment, 
            request.ReviewImageUrl), cancellationToken);
        return Ok(review);
    }

    public record CreateReviewRequest(
        string? FullName,
        string? PhoneNumber,
        string? Email,
        int Rating,
        string Comment,
        string? ReviewImageUrl);
}

