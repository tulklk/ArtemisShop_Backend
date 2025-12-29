using AtermisShop.Application.Vouchers.Common;
using MediatR;

namespace AtermisShop.Application.Vouchers.Queries.GetVoucherById;

public sealed record GetVoucherByIdQuery(Guid Id) : IRequest<VoucherDto?>;

