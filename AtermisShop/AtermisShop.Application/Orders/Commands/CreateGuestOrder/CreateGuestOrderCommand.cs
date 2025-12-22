using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Orders.Commands.CreateGuestOrder;

public sealed record CreateGuestOrderCommand(
    string GuestEmail,
    string GuestPhone,
    string GuestName,
    string? ShippingAddress,
    string? Notes,
    List<GuestOrderItem> Items,
    string? VoucherCode = null) : IRequest<Order>;

public sealed record GuestOrderItem(
    Guid ProductId,
    int Quantity);

