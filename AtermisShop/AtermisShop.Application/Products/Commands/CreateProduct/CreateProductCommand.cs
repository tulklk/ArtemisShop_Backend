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
    string? Model3DUrl,
    List<CreateProductImageDto>? Images,
    List<ProductVariantDto>? Variants) : IRequest<Guid>;

public enum ProductImageTypeDto
{
    Product = 0,
    Illustration = 1
}

public sealed record CreateProductImageDto(
    string ImageUrl,
    ProductImageTypeDto? Type = null);

public sealed record ProductVariantDto(
    string? Color,
    string? Size,
    string? Spec,
    decimal Price,
    decimal? OriginalPrice,
    int StockQuantity,
    bool IsActive);


