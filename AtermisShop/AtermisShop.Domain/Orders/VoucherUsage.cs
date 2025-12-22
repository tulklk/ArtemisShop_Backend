using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Orders;

public class VoucherUsage : BaseEntity
{
    public Guid VoucherId { get; set; }
    public Voucher Voucher { get; set; } = default!;
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? GuestEmail { get; set; }
}

