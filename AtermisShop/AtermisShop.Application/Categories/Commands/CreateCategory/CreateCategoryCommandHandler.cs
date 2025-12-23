using AtermisShop.Application.Categories.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ProductCategory
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = null
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Create child categories if provided
        if (request.Children != null && request.Children.Any())
        {
            var childCategories = request.Children.Select(childName => new ProductCategory
            {
                Name = childName,
                Description = null,
                ParentId = category.Id
            }).ToList();

            _context.ProductCategories.AddRange(childCategories);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Reload category with children and products to get accurate counts
        var categoryWithRelations = await _context.ProductCategories
            .Include(c => c.Children)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

        if (categoryWithRelations == null)
            throw new InvalidOperationException("Category not found after creation");

        return new CategoryDto
        {
            Id = categoryWithRelations.Id,
            Name = categoryWithRelations.Name,
            Description = categoryWithRelations.Description,
            ProductCount = categoryWithRelations.Products.Count,
            Children = categoryWithRelations.Children.Select(c => c.Name).ToList()
        };
    }
}


