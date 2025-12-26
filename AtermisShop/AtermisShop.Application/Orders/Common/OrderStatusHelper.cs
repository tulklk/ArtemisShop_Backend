namespace AtermisShop.Application.Orders.Common;

public static class OrderStatusHelper
{
    public const string Processing = "Processing";
    public const string Confirmed = "Confirmed";
    public const string Preparing = "Preparing";
    public const string Shipped = "Shipped";
    public const string Completed = "Completed";
    public const string Canceled = "Canceled";

    public static int FromString(string status)
    {
        return status switch
        {
            Processing => 0,
            Confirmed => 1,
            Preparing => 2,
            Shipped => 3,
            Completed => 4,
            Canceled => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown order status: {status}")
        };
    }

    public static string FromInt(int status)
    {
        return status switch
        {
            0 => Processing,
            1 => Confirmed,
            2 => Preparing,
            3 => Shipped,
            4 => Completed,
            5 => Canceled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown order status integer: {status}")
        };
    }

    public static bool IsValid(string status)
    {
        return status == Processing ||
               status == Confirmed ||
               status == Preparing ||
               status == Shipped ||
               status == Completed ||
               status == Canceled;
    }
}

