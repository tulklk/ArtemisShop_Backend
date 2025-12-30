using AtermisShop.Application.Common.Helpers;
using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Cart.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateCartItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        // Validate engraving text if provided
        if (!EngravingTextValidator.TryValidate(request.EngravingText, out var errorMessage))
        {
            throw new ArgumentException(errorMessage ?? "Invalid engraving text");
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new InvalidOperationException("Cart item not found");

        cartItem.Quantity = request.Quantity;

        // Normalize engraving text (trim and convert to uppercase)
        cartItem.EngravingText = string.IsNullOrWhiteSpace(request.EngravingText)
            ? null
            : request.EngravingText.Trim().ToUpperInvariant();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

