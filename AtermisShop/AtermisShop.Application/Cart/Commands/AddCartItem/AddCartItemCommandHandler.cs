using AtermisShop.Application.Common.Helpers;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Cart.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddCartItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        // Validate engraving text if provided
        if (!EngravingTextValidator.TryValidate(request.EngravingText, out var errorMessage))
        {
            throw new ArgumentException(errorMessage ?? "Invalid engraving text");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Domain.Orders.Cart
            {
                UserId = request.UserId
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Normalize engraving text (trim and convert to uppercase)
        var normalizedEngravingText = string.IsNullOrWhiteSpace(request.EngravingText)
            ? null
            : request.EngravingText.Trim().ToUpperInvariant();

        // Find existing item with same product and engraving text
        var existingItem = cart.Items.FirstOrDefault(i => 
            i.ProductId == request.ProductId && 
            i.EngravingText == normalizedEngravingText);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                EngravingText = normalizedEngravingText,
                UnitPriceSnapshot = product.Price
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return cart.Id;
    }
}

