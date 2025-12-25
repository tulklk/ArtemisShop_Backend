namespace AtermisShop_API.Controllers.Payments;

public sealed class PayOsConfirmRequest
{
    public string Code { get; set; } = default!;
    public string Id { get; set; } = default!;
    public bool Cancel { get; set; }
    public string Status { get; set; } = default!;
    public long OrderCode { get; set; }
    public Guid OrderId { get; set; }
}

