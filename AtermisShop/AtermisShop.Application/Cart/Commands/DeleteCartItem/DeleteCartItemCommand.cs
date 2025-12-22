using MediatR;

namespace AtermisShop.Application.Cart.Commands.DeleteCartItem;

public sealed record DeleteCartItemCommand(Guid CartItemId) : IRequest<Unit>;

