using AtermisShop.Application.Orders.Common;
using MediatR;

namespace AtermisShop.Application.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(Guid UserId) : IRequest<IReadOnlyList<OrderDto>>;

