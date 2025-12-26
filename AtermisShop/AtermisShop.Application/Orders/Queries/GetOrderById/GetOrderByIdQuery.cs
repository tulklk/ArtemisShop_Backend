using AtermisShop.Application.Orders.Common;
using MediatR;

namespace AtermisShop.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId, Guid? UserId = null) : IRequest<OrderDto?>;

