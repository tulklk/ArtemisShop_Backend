using MediatR;

namespace AtermisShop.Application.Cart.Commands.AddCartItem;

public sealed record AddCartItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity,
    string? EngravingText = null) : IRequest<Guid>;

