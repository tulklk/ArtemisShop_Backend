namespace AtermisShop.Application.Vouchers.Common;

public static class DiscountTypeHelper
{
    public const string Fixed = "Fixed";
    public const string Percent = "Percent";

    public static string FromInt(int type)
    {
        return type switch
        {
            0 => Fixed,
            1 => Percent,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown discount type integer: {type}")
        };
    }

    public static int ToInt(string type)
    {
        return type switch
        {
            Fixed => 0,
            Percent => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown discount type: {type}")
        };
    }

    public static bool IsValid(string? type)
    {
        return type == Fixed || type == Percent;
    }
}

