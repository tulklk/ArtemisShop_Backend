using AtermisShop.Domain.Wishlist;
using MediatR;

namespace AtermisShop.Application.Wishlist.Queries.GetWishlist;

public sealed record GetWishlistQuery(Guid UserId) : IRequest<IReadOnlyList<Domain.Wishlist.Wishlist>>;

