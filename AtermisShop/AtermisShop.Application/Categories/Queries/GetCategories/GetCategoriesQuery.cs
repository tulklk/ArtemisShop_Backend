using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery() : IRequest<IReadOnlyList<ProductCategory>>;


