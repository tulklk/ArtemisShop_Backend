using MediatR;

namespace AtermisShop.Application.News.Commands.UpdateNews;

public sealed record UpdateNewsCommand(
    Guid Id,
    string Title,
    string Content,
    string? Summary,
    string? ThumbnailUrl,
    string? Category,
    string? Tags,
    bool IsPublished) : IRequest;

