using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Wishlist.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Wishlist.Queries.GetWishlist;

public sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, IReadOnlyList<WishlistDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWishlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WishlistDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var wishlistItems = await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.Images)
            .Where(w => w.UserId == request.UserId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return wishlistItems.Select(w =>
        {
            var primaryImage = w.Product.Images?.FirstOrDefault(img => img.IsPrimary)
                ?? w.Product.Images?.FirstOrDefault();

            return new WishlistDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                ProductDescription = w.Product.Description,
                ProductImageUrl = primaryImage?.ImageUrl,
                Price = w.Product.Price,
                OriginalPrice = w.Product.OriginalPrice,
                IsActive = w.Product.IsActive,
                HasVariants = w.Product.HasVariants,
                AddedAt = w.CreatedAt
            };
        }).ToList();
    }
}

