using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.News.Queries.GetNewsById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Queries.GetAllNews;

public sealed class GetAllNewsQueryHandler : IRequestHandler<GetAllNewsQuery, IReadOnlyList<NewsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllNewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NewsDto>> Handle(GetAllNewsQuery request, CancellationToken cancellationToken)
    {
        var newsPosts = await _context.NewsPosts
            .Include(n => n.Author)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return newsPosts.Select(n => new NewsDto(
            n.Id,
            n.Title,
            n.Slug,
            n.Content,
            n.Summary,
            n.ThumbnailUrl,
            n.AuthorId,
            n.Author?.FullName,
            n.Category,
            n.Tags,
            n.IsPublished,
            n.PublishedAt,
            n.ViewCount,
            n.CreatedAt,
            n.UpdatedAt)).ToList();
    }
}

