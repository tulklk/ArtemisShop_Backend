using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Wishlist.Queries.CheckWishlist;

public sealed class CheckWishlistQueryHandler : IRequestHandler<CheckWishlistQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public CheckWishlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckWishlistQuery request, CancellationToken cancellationToken)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);
    }
}

