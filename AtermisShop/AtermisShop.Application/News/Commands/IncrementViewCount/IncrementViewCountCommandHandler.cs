using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Commands.IncrementViewCount;

public sealed class IncrementViewCountCommandHandler : IRequestHandler<IncrementViewCountCommand>
{
    private readonly IApplicationDbContext _context;

    public IncrementViewCountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(IncrementViewCountCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await _context.NewsPosts
            .FirstOrDefaultAsync(n => n.Id == request.NewsId && n.IsPublished, cancellationToken);

        if (newsPost != null)
        {
            newsPost.ViewCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

