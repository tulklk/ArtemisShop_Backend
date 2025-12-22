using AtermisShop.Domain.Common;

namespace AtermisShop.Domain.Orders;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;
    public int PaymentGateway { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? TransactionId { get; set; }
    public int Status { get; set; }
    public string? RawResponse { get; set; }
}

