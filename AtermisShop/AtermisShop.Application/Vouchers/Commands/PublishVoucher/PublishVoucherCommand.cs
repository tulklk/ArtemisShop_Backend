using AtermisShop.Application.Vouchers.Common;
using MediatR;

namespace AtermisShop.Application.Vouchers.Commands.PublishVoucher;

public sealed record PublishVoucherCommand(Guid Id, bool IsPublic) : IRequest<VoucherDto>;

