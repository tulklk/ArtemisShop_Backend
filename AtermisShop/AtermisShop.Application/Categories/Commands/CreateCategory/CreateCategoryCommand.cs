using MediatR;

namespace AtermisShop.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string Slug, Guid? ParentId) : IRequest<Guid>;


