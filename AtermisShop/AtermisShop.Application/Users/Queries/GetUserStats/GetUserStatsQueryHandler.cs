using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Users.Queries.GetUserStats;

public sealed class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var totalOrders = await _context.Orders
            .CountAsync(o => o.UserId == request.UserId, cancellationToken);

        var totalSpent = await _context.Orders
            .Where(o => o.UserId == request.UserId && o.OrderStatus != (int)Domain.Orders.OrderStatus.Canceled)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        // Saved designs - placeholder for future implementation
        var savedDesigns = 0;

        return new UserStatsDto(totalOrders, savedDesigns, totalSpent);
    }
}

