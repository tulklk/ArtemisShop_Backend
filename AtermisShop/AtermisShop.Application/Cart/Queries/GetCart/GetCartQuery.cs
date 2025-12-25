using AtermisShop.Application.Cart.Common;
using MediatR;

namespace AtermisShop.Application.Cart.Queries.GetCart;

public sealed record GetCartQuery(Guid UserId) : IRequest<CartDto?>;

