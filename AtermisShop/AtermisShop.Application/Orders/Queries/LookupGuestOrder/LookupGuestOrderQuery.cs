using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Orders.Queries.LookupGuestOrder;

public sealed record LookupGuestOrderQuery(string OrderNumber, string? Email = null) : IRequest<Order?>;

