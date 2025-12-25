using MediatR;

namespace AtermisShop.Application.Orders.Commands.ApplyVoucher;

public sealed record ApplyVoucherCommand(
    string Code,
    Guid? UserId = null,
    decimal? OrderAmount = null) : IRequest<ApplyVoucherResult>;

public sealed record ApplyVoucherResult(
    bool IsValid,
    decimal DiscountAmount,
    string? ErrorMessage = null);

