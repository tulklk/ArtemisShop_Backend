using AtermisShop.Application.Products.Reviews.Common;
using MediatR;

namespace AtermisShop.Application.Products.Reviews.Commands.CreateProductReview;

public sealed record CreateProductReviewCommand(
    Guid? UserId,
    string ProductIdOrSlug,
    string? FullName,
    string? PhoneNumber,
    string? Email,
    int Rating,
    string Comment,
    string? ReviewImageUrl) : IRequest<ProductReviewDto>;

