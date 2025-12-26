using AtermisShop.Domain.Common;
using AtermisShop.Domain.Users;

namespace AtermisShop.Domain.Orders;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = default!;
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestFullName { get; set; }
    public decimal TotalAmount { get; set; }
    public int PaymentStatus { get; set; }
    public int OrderStatus { get; set; }
    public int PaymentMethod { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? ShippingFullName { get; set; }
    public string? ShippingPhoneNumber { get; set; }
    public string? ShippingAddressLine { get; set; }
    public string? ShippingWard { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? ShippingCity { get; set; }
    public Guid? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
    public decimal? VoucherDiscountAmount { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

