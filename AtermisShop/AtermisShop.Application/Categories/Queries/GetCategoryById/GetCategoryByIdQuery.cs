using AtermisShop.Domain.Products;
using MediatR;

namespace AtermisShop.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<ProductCategory?>;

