using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Queries.GetNewsById;

public sealed class GetNewsByIdQueryHandler : IRequestHandler<GetNewsByIdQuery, NewsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetNewsByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NewsDto?> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
    {
        var newsPost = await _context.NewsPosts
            .Include(n => n.Author)
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (newsPost == null)
            return null;

        return new NewsDto(
            newsPost.Id,
            newsPost.Title,
            newsPost.Slug,
            newsPost.Content,
            newsPost.Summary,
            newsPost.ThumbnailUrl,
            newsPost.AuthorId,
            newsPost.Author?.FullName,
            newsPost.Category,
            newsPost.Tags,
            newsPost.IsPublished,
            newsPost.PublishedAt,
            newsPost.ViewCount,
            newsPost.CreatedAt,
            newsPost.UpdatedAt,
            newsPost.NewsUrl);
    }
}

