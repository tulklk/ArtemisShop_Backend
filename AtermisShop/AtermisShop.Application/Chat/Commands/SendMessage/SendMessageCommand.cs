using MediatR;

namespace AtermisShop.Application.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid? UserId,
    string Message,
    string? SessionId = null) : IRequest<Guid>;

