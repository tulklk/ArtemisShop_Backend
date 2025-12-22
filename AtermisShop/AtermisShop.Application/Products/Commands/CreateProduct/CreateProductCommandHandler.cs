using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Price = request.Price,
            OriginalPrice = request.Price,
            StockQuantity = 0,
            IsActive = true,
            HasVariants = false,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}


