using AtermisShop.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Common;

public static class OrderNumberHelper
{
    private static readonly Random _random = new Random();
    private const int OrderNumberLength = 7;
    private const int MaxAttempts = 100;

    /// <summary>
    /// Generates a unique 7-digit order number
    /// </summary>
    public static async Task<string> GenerateUniqueOrderNumberAsync(
        IApplicationDbContext context, 
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var orderNumber = GenerateOrderNumber();
            
            // Check if order number already exists
            var exists = await context.Orders
                .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
            
            if (!exists)
            {
                return orderNumber;
            }
        }

        throw new InvalidOperationException("Unable to generate unique order number after multiple attempts.");
    }

    /// <summary>
    /// Generates a random 7-digit order number
    /// </summary>
    private static string GenerateOrderNumber()
    {
        // Generate a random number between 1000000 and 9999999 (7 digits)
        var number = _random.Next(1000000, 9999999);
        return number.ToString();
    }
}

