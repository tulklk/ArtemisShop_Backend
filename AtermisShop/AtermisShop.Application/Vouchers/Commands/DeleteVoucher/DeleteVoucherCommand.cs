using MediatR;

namespace AtermisShop.Application.Vouchers.Commands.DeleteVoucher;

public sealed record DeleteVoucherCommand(Guid Id) : IRequest;

