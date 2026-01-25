using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.News.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Queries.GetNews;

public sealed class GetNewsQueryHandler : IRequestHandler<GetNewsQuery, IReadOnlyList<NewsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NewsDto>> Handle(GetNewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NewsPosts
            .AsNoTracking()
            .Include(n => n.Author)
            .Where(n => n.IsPublished)
            .AsQueryable();

        // Search by title or content
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(n => 
                n.Title.ToLower().Contains(searchTerm) || 
                (n.Summary != null && n.Summary.ToLower().Contains(searchTerm)) ||
                n.Content.ToLower().Contains(searchTerm));
        }

        // Order by published date
        query = query.OrderByDescending(n => n.PublishedAt ?? n.CreatedAt);

        // Pagination
        if (request.Page.HasValue && request.PageSize.HasValue)
        {
            var page = request.Page.Value > 0 ? request.Page.Value : 1;
            var pageSize = request.PageSize.Value > 0 ? request.PageSize.Value : 10;
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        var newsPosts = await query.ToListAsync(cancellationToken);

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
            n.PublishedAt,
            n.ViewCount,
            n.CreatedAt)).ToList();
    }
}

