namespace AtermisShop_API.Controllers.Payments;

public sealed class PayOsWebhookRequest
{
    public string Code { get; set; } = default!;
    public string Desc { get; set; } = default!;
    public bool Success { get; set; }
    public PayOsWebhookData? Data { get; set; }
    public string Signature { get; set; } = default!;
}

public sealed class PayOsWebhookData
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string Reference { get; set; } = default!;
    public string TransactionDateTime { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public string PaymentLinkId { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Desc { get; set; } = default!;
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
}

