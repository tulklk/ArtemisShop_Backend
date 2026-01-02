using MediatR;

namespace AtermisShop.Application.News.Commands.CreateNews;

public sealed record CreateNewsCommand(
    string Title,
    string Content,
    string? Summary,
    string? ThumbnailUrl,
    string? Category,
    string? Tags,
    bool IsPublished,
    Guid? AuthorId = null) : IRequest<CreateNewsResult>;

public sealed record CreateNewsResult(
    Guid Id,
    string Title,
    string Slug);

