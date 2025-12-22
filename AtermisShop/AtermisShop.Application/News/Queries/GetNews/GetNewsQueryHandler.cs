using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.News;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Queries.GetNews;

public sealed class GetNewsQueryHandler : IRequestHandler<GetNewsQuery, IReadOnlyList<NewsPost>>
{
    private readonly IApplicationDbContext _context;

    public GetNewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NewsPost>> Handle(GetNewsQuery request, CancellationToken cancellationToken)
    {
        return await _context.NewsPosts
            .Where(n => n.IsPublished)
            .OrderByDescending(n => n.PublishedAt ?? n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

