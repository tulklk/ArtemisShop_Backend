using MediatR;

namespace AtermisShop.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    Guid CategoryId) : IRequest<Guid>;


