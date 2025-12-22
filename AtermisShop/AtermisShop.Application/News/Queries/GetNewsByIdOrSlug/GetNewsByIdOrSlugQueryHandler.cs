using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.News;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Queries.GetNewsByIdOrSlug;

public sealed class GetNewsByIdOrSlugQueryHandler : IRequestHandler<GetNewsByIdOrSlugQuery, NewsPost?>
{
    private readonly IApplicationDbContext _context;

    public GetNewsByIdOrSlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NewsPost?> Handle(GetNewsByIdOrSlugQuery request, CancellationToken cancellationToken)
    {
        var isGuid = Guid.TryParse(request.IdOrSlug, out var id);
        
        return await _context.NewsPosts
            .Where(n => n.IsPublished && (isGuid ? n.Id == id : n.Slug == request.IdOrSlug))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

