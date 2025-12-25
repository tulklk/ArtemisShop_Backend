namespace AtermisShop.Application.Orders.Common;

public static class PaymentMethod
{
    public const string COD = "COD";
    public const string PayOS = "PayOS";

    public static bool IsValid(string? method)
    {
        return method == COD || method == PayOS;
    }

    public static int ToInt(string method)
    {
        return method switch
        {
            COD => 0,
            PayOS => 1,
            _ => throw new ArgumentException($"Invalid payment method: {method}")
        };
    }
}

