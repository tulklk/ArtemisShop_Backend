using AtermisShop.Application.Wishlist.Common;
using MediatR;

namespace AtermisShop.Application.Wishlist.Commands.AddToWishlist;

public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<WishlistDto>;

