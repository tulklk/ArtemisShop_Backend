namespace AtermisShop.Application.Orders.Common;

public sealed class OrderDto
{
    public Guid Id { get; set; }
    public Guid OrderNumber { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
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
    public string? VoucherCode { get; set; }
    public decimal? VoucherDiscountAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = default!;
    public string? VariantInfoSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

