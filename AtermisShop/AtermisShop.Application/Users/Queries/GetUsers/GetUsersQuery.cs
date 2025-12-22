using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery() : IRequest<IReadOnlyList<ApplicationUser>>;

