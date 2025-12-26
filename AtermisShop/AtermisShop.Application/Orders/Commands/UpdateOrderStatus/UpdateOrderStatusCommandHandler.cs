using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Orders.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (!OrderStatusHelper.IsValid(request.Status))
        {
            throw new ArgumentException($"Invalid order status: {request.Status}. Valid statuses are: {OrderStatusHelper.Processing}, {OrderStatusHelper.Confirmed}, {OrderStatusHelper.Preparing}, {OrderStatusHelper.Shipped}, {OrderStatusHelper.Completed}, {OrderStatusHelper.Canceled}");
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found.");
        }

        order.OrderStatus = OrderStatusHelper.FromString(request.Status);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

