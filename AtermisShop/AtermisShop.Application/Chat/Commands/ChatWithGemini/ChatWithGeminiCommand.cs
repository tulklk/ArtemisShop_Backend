using AtermisShop.Application.Chat.Common;
using MediatR;

namespace AtermisShop.Application.Chat.Commands.ChatWithGemini;

public sealed record ChatWithGeminiCommand(
    string Message,
    string? SessionId = null,
    Guid? UserId = null) : IRequest<ChatWithGeminiResult>;

public sealed record ChatWithGeminiResult(
    string Response,
    string SessionId,
    List<SuggestedProductDto> SuggestedProducts);

