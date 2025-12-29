using AtermisShop.Application.Vouchers.Common;
using MediatR;

namespace AtermisShop.Application.Vouchers.Commands.UpdateVoucher;

public sealed record UpdateVoucherCommand(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal? MinOrderAmount,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPublic,
    int UsageLimitTotal,
    int UsageLimitPerCustomer) : IRequest<VoucherDto>;

