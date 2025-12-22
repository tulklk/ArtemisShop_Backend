using MediatR;

namespace AtermisShop.Application.Users.Queries.GetUserStats;

public sealed record GetUserStatsQuery(Guid UserId) : IRequest<UserStatsDto>;

public sealed record UserStatsDto(
    int TotalOrders,
    int SavedDesigns,
    decimal TotalSpent);

