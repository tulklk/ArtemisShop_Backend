using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Cart.Queries.GetCart;

public sealed record GetCartQuery(Guid UserId) : IRequest<Domain.Orders.Cart?>;

