using MediatR;

namespace AtermisShop.Application.Cart.Commands.UpdateCartItem;

public sealed record UpdateCartItemCommand(
    Guid CartItemId,
    int Quantity) : IRequest<Unit>;

