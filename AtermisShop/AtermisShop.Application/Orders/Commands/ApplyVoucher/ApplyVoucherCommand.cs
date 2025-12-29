using MediatR;

namespace AtermisShop.Application.Orders.Commands.ApplyVoucher;

public sealed record ApplyVoucherCommand(
    string Code,
    Guid? UserId = null,
    decimal? OrderAmount = null,
    List<GuestOrderItem>? GuestItems = null) : IRequest<ApplyVoucherResult>;

public sealed record GuestOrderItem(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);

public sealed record ApplyVoucherResult(
    bool IsValid,
    decimal DiscountAmount,
    string? ErrorMessage = null);

