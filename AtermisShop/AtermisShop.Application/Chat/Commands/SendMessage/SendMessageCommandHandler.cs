using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Chat;
using MediatR;

namespace AtermisShop.Application.Chat.Commands.SendMessage;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SendMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new ChatMessage
        {
            UserId = request.UserId,
            Message = request.Message,
            SessionId = request.SessionId ?? Guid.NewGuid().ToString()
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return message.Id;
    }
}

