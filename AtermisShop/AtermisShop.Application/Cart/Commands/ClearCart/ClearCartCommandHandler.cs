using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Cart.Commands.ClearCart;

public sealed class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ClearCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart == null)
            return Unit.Value;

        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

