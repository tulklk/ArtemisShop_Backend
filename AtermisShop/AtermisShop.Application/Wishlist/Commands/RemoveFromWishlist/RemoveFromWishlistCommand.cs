using MediatR;

namespace AtermisShop.Application.Wishlist.Commands.RemoveFromWishlist;

public sealed record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest<Unit>;

