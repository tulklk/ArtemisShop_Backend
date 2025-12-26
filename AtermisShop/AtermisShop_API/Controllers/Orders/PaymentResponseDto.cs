namespace AtermisShop_API.Controllers.Orders;

public sealed class PaymentResponseDto
{
    public string PaymentUrl { get; set; } = default!;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public bool IsCod { get; set; }
    public bool IsBankTransfer { get; set; }
    public BankTransferInfoDto? BankTransferInfo { get; set; }
}

public sealed class BankTransferInfoDto
{
    public string? QrDataUrl { get; set; }
    public string? QrCode { get; set; }
    public string? BankName { get; set; }
    public string? AccountNo { get; set; }
    public string? AccountName { get; set; }
    public decimal Amount { get; set; }
    public string? TransferContent { get; set; }
}

