using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ProductCategory
        {
            Name = request.Name,
            Description = null,
            ParentId = request.ParentId
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}


