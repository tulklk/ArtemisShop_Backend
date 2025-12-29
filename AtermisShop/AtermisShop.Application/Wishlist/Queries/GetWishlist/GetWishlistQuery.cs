using AtermisShop.Application.Wishlist.Common;
using MediatR;

namespace AtermisShop.Application.Wishlist.Queries.GetWishlist;

public sealed record GetWishlistQuery(Guid UserId) : IRequest<IReadOnlyList<WishlistDto>>;

