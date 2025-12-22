using MediatR;

namespace AtermisShop.Application.Cart.Commands.ClearCart;

public sealed record ClearCartCommand(Guid UserId) : IRequest<Unit>;

