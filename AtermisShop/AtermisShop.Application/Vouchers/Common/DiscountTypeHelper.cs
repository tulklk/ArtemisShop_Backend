namespace AtermisShop.Application.Vouchers.Common;

public static class DiscountTypeHelper
{
    public const string Amount = "Amount";
    public const string Percent = "Percent";

    public static string FromInt(int type)
    {
        return type switch
        {
            0 => Amount,
            1 => Percent,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown discount type integer: {type}")
        };
    }

    public static int ToInt(string type)
    {
        return type switch
        {
            Amount => 0,
            Percent => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown discount type: {type}")
        };
    }

    public static bool IsValid(string? type)
    {
        return type == Amount || type == Percent;
    }
}

