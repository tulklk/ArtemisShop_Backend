using AtermisShop.Domain.Users;
using MediatR;

namespace AtermisShop.Application.Auth.Queries.GetMe;

public sealed record GetMeQuery(Guid UserId) : IRequest<ApplicationUser?>;

