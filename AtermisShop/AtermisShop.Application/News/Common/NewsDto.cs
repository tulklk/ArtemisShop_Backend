namespace AtermisShop.Application.News.Common;

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
    DateTime? PublishedAt,
    int ViewCount,
    DateTime CreatedAt);

