namespace AtermisShop.Application.Payments.Common;

public sealed record PaymentItem(
    string Name,
    int Quantity,
    int Price); // Price in VND (integer)

