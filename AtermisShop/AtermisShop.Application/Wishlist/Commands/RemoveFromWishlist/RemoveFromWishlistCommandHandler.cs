using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Wishlist.Commands.RemoveFromWishlist;

public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RemoveFromWishlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        if (item != null)
        {
            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

