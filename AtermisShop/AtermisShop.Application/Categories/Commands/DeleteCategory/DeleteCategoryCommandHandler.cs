using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .Include(c => c.Children)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new InvalidOperationException("Category not found");

        // Check if category has products
        if (category.Products.Any())
            throw new InvalidOperationException("Cannot delete category that has products. Please remove all products first.");

        // Delete all children recursively first
        if (category.Children.Any())
        {
            await DeleteChildrenRecursive(category.Children.ToList(), cancellationToken);
        }

        // Delete the category itself
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteChildrenRecursive(List<ProductCategory> children, CancellationToken cancellationToken)
    {
        foreach (var child in children)
        {
            // Load child's children and products to check and delete recursively
            var childWithRelations = await _context.ProductCategories
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == child.Id, cancellationToken);

            if (childWithRelations == null)
                continue;

            // Check if child has products
            if (childWithRelations.Products.Any())
                throw new InvalidOperationException($"Cannot delete category '{childWithRelations.Name}' that has products. Please remove all products first.");

            // Recursively delete grandchildren first
            if (childWithRelations.Children.Any())
            {
                await DeleteChildrenRecursive(childWithRelations.Children.ToList(), cancellationToken);
            }

            // Delete the child
            _context.ProductCategories.Remove(childWithRelations);
        }
        
        // Save changes after deleting all children at this level
        await _context.SaveChangesAsync(cancellationToken);
    }
}

