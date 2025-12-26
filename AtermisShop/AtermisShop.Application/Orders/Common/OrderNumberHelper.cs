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
        // Use timestamp-based approach to reduce collisions
        var baseNumber = (int)(DateTime.UtcNow.Ticks % 10000000); // Last 7 digits of ticks
        if (baseNumber < 1000000) baseNumber += 1000000; // Ensure 7 digits
        
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            string orderNumber;
            
            if (attempt == 0)
            {
                // First attempt: use timestamp-based number
                orderNumber = baseNumber.ToString();
            }
            else
            {
                // Subsequent attempts: add random offset
                var offset = _random.Next(1, 1000);
                var number = (baseNumber + offset) % 10000000;
                if (number < 1000000) number += 1000000;
                orderNumber = number.ToString();
            }
            
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
}

