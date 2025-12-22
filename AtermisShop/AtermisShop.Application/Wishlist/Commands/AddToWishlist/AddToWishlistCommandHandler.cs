using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Wishlist;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Wishlist.Commands.AddToWishlist;

public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public AddToWishlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Wishlists
            .AnyAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        if (exists)
            return Unit.Value;

        var wishlistItem = new Domain.Wishlist.Wishlist
        {
            UserId = request.UserId,
            ProductId = request.ProductId
        };

        _context.Wishlists.Add(wishlistItem);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

