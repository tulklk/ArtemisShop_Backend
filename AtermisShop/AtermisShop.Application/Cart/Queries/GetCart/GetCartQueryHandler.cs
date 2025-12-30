using AtermisShop.Application.Cart.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Cart.Queries.GetCart;

public sealed class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null) return null;

        var cartDto = new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item =>
            {
                var variantInfo = string.Empty;
                if (item.ProductVariant != null)
                {
                    var parts = new List<string>();
                    if (!string.IsNullOrEmpty(item.ProductVariant.Color))
                        parts.Add($"Color: {item.ProductVariant.Color}");
                    if (!string.IsNullOrEmpty(item.ProductVariant.Size))
                        parts.Add($"Size: {item.ProductVariant.Size}");
                    if (!string.IsNullOrEmpty(item.ProductVariant.Spec))
                        parts.Add($"Spec: {item.ProductVariant.Spec}");
                    variantInfo = string.Join(", ", parts);
                }

                return new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.Product.Name,
                    ProductImageUrl = item.Product.Images.FirstOrDefault()?.ImageUrl,
                    VariantInfo = string.IsNullOrEmpty(variantInfo) ? null : variantInfo,
                    UnitPrice = item.UnitPriceSnapshot,
                    Quantity = item.Quantity,
                    LineTotal = item.Quantity * item.UnitPriceSnapshot,
                    EngravingText = item.EngravingText
                };
            }).ToList()
        };

        cartDto.SubTotal = cartDto.Items.Sum(i => i.LineTotal);
        cartDto.TotalItems = cartDto.Items.Sum(i => i.Quantity);

        return cartDto;
    }
}

