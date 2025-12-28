namespace AtermisShop.Application.Orders.Common;

public static class PaymentStatusHelper
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";

    public static string FromInt(int status)
    {
        return status switch
        {
            0 => Pending,
            1 => Paid,
            _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown payment status integer: {status}")
        };
    }

    public static int ToInt(string status)
    {
        return status switch
        {
            Pending => 0,
            Paid => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown payment status: {status}")
        };
    }
}

