using MediatR;

namespace AtermisShop.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    Guid CategoryId,
    decimal? OriginalPrice = null,
    int? StockQuantity = null,
    string? Brand = null,
    bool? IsActive = null) : IRequest;

