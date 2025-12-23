using AtermisShop.Application.Categories.Common;
using MediatR;

namespace AtermisShop.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description, List<string>? Children) : IRequest<CategoryDto>;


