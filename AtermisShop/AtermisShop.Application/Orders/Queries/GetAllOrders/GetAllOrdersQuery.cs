using AtermisShop.Application.Orders.Common;
using MediatR;

namespace AtermisShop.Application.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery() : IRequest<IReadOnlyList<OrderDto>>;

