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
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new InvalidOperationException("Cart item not found");

        cartItem.Quantity = request.Quantity;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

