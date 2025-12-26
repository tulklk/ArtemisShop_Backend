using MediatR;

namespace AtermisShop.Application.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    string Status) : IRequest;
