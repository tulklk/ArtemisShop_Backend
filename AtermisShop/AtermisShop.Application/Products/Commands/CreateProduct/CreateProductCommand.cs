using MediatR;

namespace AtermisShop.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    decimal? OriginalPrice,
    int StockQuantity,
    string? Brand,
    bool IsActive,
    bool HasEngraving,
    string? DefaultEngravingText,
    List<string>? ImageUrls,
    List<ProductVariantDto>? Variants) : IRequest<Guid>;

public sealed record ProductVariantDto(
    string? Color,
    string? Size,
    string? Spec,
    decimal Price,
    decimal? OriginalPrice,
    int StockQuantity,
    bool IsActive);


