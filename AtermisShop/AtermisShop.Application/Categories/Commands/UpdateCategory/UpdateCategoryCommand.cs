using MediatR;

namespace AtermisShop.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    List<ChildCategoryDto>? Children) : IRequest;

public sealed record ChildCategoryDto(string Name, string? Description, List<ChildCategoryDto>? Children);

