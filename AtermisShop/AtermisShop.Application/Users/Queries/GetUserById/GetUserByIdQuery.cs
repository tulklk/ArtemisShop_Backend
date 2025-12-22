using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<ApplicationUser?>;

