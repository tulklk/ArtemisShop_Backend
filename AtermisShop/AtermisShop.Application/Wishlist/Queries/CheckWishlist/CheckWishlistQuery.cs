using MediatR;

namespace AtermisShop.Application.Wishlist.Queries.CheckWishlist;

public sealed record CheckWishlistQuery(Guid UserId, Guid ProductId) : IRequest<bool>;

