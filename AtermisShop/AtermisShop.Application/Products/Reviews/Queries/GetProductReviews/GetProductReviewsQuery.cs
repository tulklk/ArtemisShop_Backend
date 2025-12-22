using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery(string ProductIdOrSlug) : IRequest<IReadOnlyList<ProductReview>>;

