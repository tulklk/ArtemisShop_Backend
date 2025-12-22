using MediatR;

namespace AtermisShop.Application.Orders.Commands.ApplyVoucher;

public sealed record ApplyVoucherCommand(
    string VoucherCode,
    decimal OrderAmount) : IRequest<ApplyVoucherResult>;

public sealed record ApplyVoucherResult(
    bool IsValid,
    decimal DiscountAmount,
    string? ErrorMessage = null);

