using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new InvalidOperationException("Category not found");

        // Update main category
        category.Name = request.Name;
        category.Description = request.Description;

        // Handle children updates
        if (request.Children != null)
        {
            // If children is provided (even if empty), replace all existing children
            // Remove existing children that are not in the new list
            var existingChildNames = request.Children.Select(c => c.Name).ToList();
            var childrenToRemove = category.Children
                .Where(c => !existingChildNames.Contains(c.Name))
                .ToList();

            foreach (var childToRemove in childrenToRemove)
            {
                _context.ProductCategories.Remove(childToRemove);
            }

            // Update or create children
            foreach (var childDto in request.Children)
            {
                var existingChild = category.Children.FirstOrDefault(c => c.Name == childDto.Name);
                
                if (existingChild != null)
                {
                    // Update existing child
                    existingChild.Name = childDto.Name;
                    existingChild.Description = childDto.Description;
                    await UpdateChildrenRecursive(existingChild, childDto.Children, cancellationToken);
                }
                else
                {
                    // Create new child
                    var newChild = new ProductCategory
                    {
                        Name = childDto.Name,
                        Description = childDto.Description,
                        ParentId = category.Id
                    };
                    _context.ProductCategories.Add(newChild);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    if (childDto.Children != null && childDto.Children.Any())
                    {
                        await UpdateChildrenRecursive(newChild, childDto.Children, cancellationToken);
                    }
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateChildrenRecursive(ProductCategory parent, List<ChildCategoryDto>? children, CancellationToken cancellationToken)
    {
        if (children == null || !children.Any())
            return;

        // Reload parent with children to ensure we have the latest data
        var parentWithChildren = await _context.ProductCategories
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == parent.Id, cancellationToken);

        if (parentWithChildren == null)
            return;

        var existingChildNames = children.Select(c => c.Name).ToList();
        var childrenToRemove = parentWithChildren.Children
            .Where(c => !existingChildNames.Contains(c.Name))
            .ToList();

        foreach (var childToRemove in childrenToRemove)
        {
            _context.ProductCategories.Remove(childToRemove);
        }

        foreach (var childDto in children)
        {
            var existingChild = parentWithChildren.Children.FirstOrDefault(c => c.Name == childDto.Name);
            
            if (existingChild != null)
            {
                existingChild.Name = childDto.Name;
                existingChild.Description = childDto.Description;
                await UpdateChildrenRecursive(existingChild, childDto.Children, cancellationToken);
            }
            else
            {
                var newChild = new ProductCategory
                {
                    Name = childDto.Name,
                    Description = childDto.Description,
                    ParentId = parentWithChildren.Id
                };
                _context.ProductCategories.Add(newChild);
                await _context.SaveChangesAsync(cancellationToken);
                
                if (childDto.Children != null && childDto.Children.Any())
                {
                    await UpdateChildrenRecursive(newChild, childDto.Children, cancellationToken);
                }
            }
        }
    }
}

