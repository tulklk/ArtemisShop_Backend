using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.News.Commands.DeleteNews;

public sealed class DeleteNewsCommandHandler : IRequestHandler<DeleteNewsCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteNewsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
    {
        var newsPost = await _context.NewsPosts
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (newsPost == null)
            throw new InvalidOperationException("News post not found");

        _context.NewsPosts.Remove(newsPost);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

