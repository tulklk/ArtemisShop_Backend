using MediatR;

namespace AtermisShop.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string? FullName,
    string? PhoneNumber) : IRequest<Unit>;

