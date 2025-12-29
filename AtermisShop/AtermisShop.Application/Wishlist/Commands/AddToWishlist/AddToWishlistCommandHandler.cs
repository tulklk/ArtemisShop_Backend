using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Wishlist.Common;
using AtermisShop.Domain.Wishlist;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Wishlist.Commands.AddToWishlist;

public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, WishlistDto>
{
    private readonly IApplicationDbContext _context;

    public AddToWishlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistDto> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        // Check if product exists
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        // Check if already in wishlist
        var existingItem = await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        if (existingItem != null)
        {
            // Return existing item as DTO
            var primaryImage = existingItem.Product.Images?.FirstOrDefault(img => img.IsPrimary)
                ?? existingItem.Product.Images?.FirstOrDefault();
            
            return new WishlistDto
            {
                Id = existingItem.Id,
                ProductId = existingItem.ProductId,
                ProductName = existingItem.Product.Name,
                ProductDescription = existingItem.Product.Description,
                ProductImageUrl = primaryImage?.ImageUrl,
                Price = existingItem.Product.Price,
                OriginalPrice = existingItem.Product.OriginalPrice,
                IsActive = existingItem.Product.IsActive,
                HasVariants = existingItem.Product.HasVariants,
                AddedAt = existingItem.CreatedAt
            };
        }

        // Create new wishlist item
        var wishlistItem = new Domain.Wishlist.Wishlist
        {
            UserId = request.UserId,
            ProductId = request.ProductId
        };

        _context.Wishlists.Add(wishlistItem);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with product data
        var wishlistWithProduct = await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(w => w.Id == wishlistItem.Id, cancellationToken);

        if (wishlistWithProduct == null)
        {
            throw new InvalidOperationException("Failed to create wishlist item");
        }

        var image = wishlistWithProduct.Product.Images?.FirstOrDefault(img => img.IsPrimary)
            ?? wishlistWithProduct.Product.Images?.FirstOrDefault();

        return new WishlistDto
        {
            Id = wishlistWithProduct.Id,
            ProductId = wishlistWithProduct.ProductId,
            ProductName = wishlistWithProduct.Product.Name,
            ProductDescription = wishlistWithProduct.Product.Description,
            ProductImageUrl = image?.ImageUrl,
            Price = wishlistWithProduct.Product.Price,
            OriginalPrice = wishlistWithProduct.Product.OriginalPrice,
            IsActive = wishlistWithProduct.Product.IsActive,
            HasVariants = wishlistWithProduct.Product.HasVariants,
            AddedAt = wishlistWithProduct.CreatedAt
        };
    }
}

