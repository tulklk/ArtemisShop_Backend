using AtermisShop.Application.Orders.Commands.CreateOrder;
using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Orders.Commands.CreateGuestOrder;

public sealed record CreateGuestOrderCommand(
    string Email,
    string FullName,
    ShippingAddressDto? ShippingAddress,
    string PaymentMethod,
    List<GuestOrderItem> Items,
    string? VoucherCode = null) : IRequest<Order>;

public sealed record GuestOrderItem(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);

