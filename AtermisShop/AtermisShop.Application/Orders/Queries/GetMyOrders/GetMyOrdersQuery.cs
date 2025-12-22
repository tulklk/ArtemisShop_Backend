using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(Guid UserId) : IRequest<IReadOnlyList<Order>>;

