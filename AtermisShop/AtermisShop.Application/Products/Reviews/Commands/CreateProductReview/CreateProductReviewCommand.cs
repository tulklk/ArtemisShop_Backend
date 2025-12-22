using MediatR;

namespace AtermisShop.Application.Products.Reviews.Commands.CreateProductReview;

public sealed record CreateProductReviewCommand(
    Guid UserId,
    string ProductIdOrSlug,
    int Rating,
    string Comment) : IRequest<Guid>;

