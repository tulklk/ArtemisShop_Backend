using MediatR;

namespace AtermisShop.Application.News.Queries.GetNewsById;

public sealed record GetNewsByIdQuery(Guid Id) : IRequest<NewsDto?>;

public sealed record NewsDto(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string? Summary,
    string? ThumbnailUrl,
    Guid? AuthorId,
    string? AuthorName,
    string? Category,
    string? Tags,
    bool IsPublished,
    DateTime? PublishedAt,
    int ViewCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

