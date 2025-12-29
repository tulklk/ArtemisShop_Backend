using AtermisShop.Application.Vouchers.Common;
using MediatR;

namespace AtermisShop.Application.Vouchers.Queries.GetVouchers;

public sealed record GetVouchersQuery() : IRequest<IReadOnlyList<VoucherDto>>;

