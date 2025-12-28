namespace AtermisShop_API.Controllers.GuestOrders;

public class OrderStatusResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string OrderStatus { get; set; } = default!;
    public string PaymentStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string StatusDescription { get; set; } = default!;
}

