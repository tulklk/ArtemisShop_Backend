using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid UserId,
    string? ShippingAddress,
    string? Notes,
    string? VoucherCode = null) : IRequest<Order>;

