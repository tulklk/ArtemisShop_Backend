using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Wishlist;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Wishlist.Queries.GetWishlist;

public sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, IReadOnlyList<Domain.Wishlist.Wishlist>>
{
    private readonly IApplicationDbContext _context;

    public GetWishlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Domain.Wishlist.Wishlist>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .Where(w => w.UserId == request.UserId)
            .ToListAsync(cancellationToken);
    }
}

