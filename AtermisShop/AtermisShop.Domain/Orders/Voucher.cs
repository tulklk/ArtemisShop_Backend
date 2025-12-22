using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Orders;

public class Voucher : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublic { get; set; }
    public int UsageLimitTotal { get; set; }
    public int UsageLimitPerCustomer { get; set; }
    public int UsedCount { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<VoucherUsage> Usages { get; set; } = new List<VoucherUsage>();
}

