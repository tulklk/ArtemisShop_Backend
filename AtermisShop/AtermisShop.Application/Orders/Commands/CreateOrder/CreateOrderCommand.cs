using AtermisShop.Domain.Orders;
using MediatR;

namespace AtermisShop.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid UserId,
    ShippingAddressDto? ShippingAddress,
    string PaymentMethod,
    string? VoucherCode = null) : IRequest<Order>;

public sealed record ShippingAddressDto(
    string FullName,
    string PhoneNumber,
    string AddressLine,
    string Ward,
    string District,
    string City);

