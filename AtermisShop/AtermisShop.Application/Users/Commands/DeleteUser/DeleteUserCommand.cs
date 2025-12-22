using MediatR;

namespace AtermisShop.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

