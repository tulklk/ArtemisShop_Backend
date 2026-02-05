using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Product not found");

        // Update product properties
        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;

        // Update OriginalPrice: if null, product is normal (no discount), otherwise use provided value
        if (request.OriginalPrice.HasValue)
            product.OriginalPrice = request.OriginalPrice.Value;
        else
            product.OriginalPrice = null; // Normal product (no discount)

        if (request.StockQuantity.HasValue)
            product.StockQuantity = request.StockQuantity.Value;

        if (!string.IsNullOrEmpty(request.Brand))
            product.Brand = request.Brand;

        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;

        if (request.HasEngraving.HasValue)
        {
            product.HasEngraving = request.HasEngraving.Value;
            // If HasEngraving is false, clear DefaultEngravingText
            if (!request.HasEngraving.Value)
            {
                product.DefaultEngravingText = null;
            }
            else if (request.DefaultEngravingText != null)
            {
                product.DefaultEngravingText = request.DefaultEngravingText;
            }
        }
        else if (request.DefaultEngravingText != null && product.HasEngraving)
        {
            // Only update DefaultEngravingText if HasEngraving is already true
            product.DefaultEngravingText = request.DefaultEngravingText;
        }

        // Update Model3DUrl: Model3DUrl is optional
        // If a non-null value is provided, update it (normalize empty string to null)
        // If null is provided (default), don't update (keep existing value)
        // Note: Since Model3DUrl has default value null, we can't distinguish between "not provided" and "set to null"
        // So we only update if a non-null value is provided
        // To clear Model3DUrl, admin can pass an empty string, which we'll normalize to null
        if (request.Model3DUrl != null)
        {
            // Normalize empty string to null
            product.Model3DUrl = string.IsNullOrWhiteSpace(request.Model3DUrl) ? null : request.Model3DUrl;
        }
        // If request.Model3DUrl is null (default), don't update - keep existing value

        // Update images: add new images or update type of existing images
        if (request.Images != null && request.Images.Count > 0)
        {
            foreach (var imageDto in request.Images)
            {
                if (string.IsNullOrWhiteSpace(imageDto.ImageUrl))
                    continue;

                // Check if image already exists for this product
                var existingImage = product.Images.FirstOrDefault(img => img.ImageUrl == imageDto.ImageUrl);

                if (existingImage != null)
                {
                    // Update type of existing image
                    var imageTypeDto = imageDto.Type ?? ProductImageTypeDto.Product;
                    existingImage.Type = (ProductImageType)imageTypeDto;
                }
                else
                {
                    // Add new image
                    var imageTypeDto = imageDto.Type ?? ProductImageTypeDto.Product;
                    var imageType = (ProductImageType)imageTypeDto;

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageDto.ImageUrl,
                        IsPrimary = product.Images.Count == 0, // Set as primary if no images exist
                        Type = imageType
                    };
                    _context.ProductImages.Add(productImage);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

