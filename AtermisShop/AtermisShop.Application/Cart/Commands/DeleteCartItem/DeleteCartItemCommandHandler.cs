using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Cart.Commands.DeleteCartItem;

public sealed class DeleteCartItemCommandHandler : IRequestHandler<DeleteCartItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteCartItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
            throw new InvalidOperationException("Cart item not found");

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

