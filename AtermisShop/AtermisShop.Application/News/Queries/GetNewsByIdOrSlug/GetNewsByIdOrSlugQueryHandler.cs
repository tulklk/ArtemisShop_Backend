using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.News.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Queries.GetNewsByIdOrSlug;

public sealed class GetNewsByIdOrSlugQueryHandler : IRequestHandler<GetNewsByIdOrSlugQuery, NewsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public GetNewsByIdOrSlugQueryHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<NewsDto?> Handle(GetNewsByIdOrSlugQuery request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.IdOrSlug, out var id);
        
        var newsPost = await _context.NewsPosts
            .Include(n => n.Author)
            .Where(n => n.IsPublished && (isGuid ? n.Id == id : n.Slug == request.IdOrSlug))
            .FirstOrDefaultAsync(cancellationToken);

        if (newsPost == null)
            return null;

        // Increment view count if requested
        if (request.IncrementViewCount)
        {
            newsPost.ViewCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }

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
            newsPost.PublishedAt,
            newsPost.ViewCount,
            newsPost.CreatedAt);
    }
}

