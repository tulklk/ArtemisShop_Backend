using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace AtermisShop.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var categoryExists = await _context.ProductCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new InvalidOperationException($"Category with Id {request.CategoryId} does not exist.");
        }

        // Generate slug from name
        var slug = GenerateSlug(request.Name);
        
        // Ensure slug is not empty
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = $"product-{Guid.NewGuid():N}";
        }
        
        // Ensure slug is unique
        var baseSlug = slug;
        var counter = 1;
        while (await _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        var product = new Product
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Price = request.Price,
            OriginalPrice = request.OriginalPrice, // If null, product is normal (no discount)
            StockQuantity = request.StockQuantity,
            Brand = request.Brand,
            IsActive = request.IsActive,
            HasVariants = request.Variants != null && request.Variants.Count > 0,
            HasEngraving = request.HasEngraving,
            DefaultEngravingText = request.HasEngraving ? request.DefaultEngravingText : null,
            // Normalize Model3DUrl: empty string becomes null
            Model3DUrl = string.IsNullOrWhiteSpace(request.Model3DUrl) ? null : request.Model3DUrl,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);

        // Add images
        if (request.Images != null && request.Images.Count > 0)
        {
            for (int i = 0; i < request.Images.Count; i++)
            {
                var imageDto = request.Images[i];
                if (!string.IsNullOrWhiteSpace(imageDto.ImageUrl))
                {
                    // Use provided Type or default to Product (0)
                    var imageTypeDto = imageDto.Type ?? ProductImageTypeDto.Product;
                    // Map DTO enum to domain enum
                    var imageType = (ProductImageType)imageTypeDto;

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageDto.ImageUrl,
                        IsPrimary = i == 0,
                        Type = imageType
                    };
                    _context.ProductImages.Add(productImage);
                }
            }
        }

        // Add variants
        if (request.Variants != null && request.Variants.Count > 0)
        {
            foreach (var variantDto in request.Variants)
            {
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Color = variantDto.Color,
                    Size = variantDto.Size,
                    Spec = variantDto.Spec,
                    Price = variantDto.Price,
                    OriginalPrice = variantDto.OriginalPrice, // If null, variant is normal (no discount)
                    StockQuantity = variantDto.StockQuantity,
                    IsActive = variantDto.IsActive
                };
                _context.ProductVariants.Add(variant);
            }
        }

        // Save all changes in one transaction
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize Vietnamese characters
        text = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in text)
        {
            var unicodeCategory = char.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        text = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // Convert to lowercase
        text = text.ToLowerInvariant();

        // Replace spaces with hyphens
        text = Regex.Replace(text, @"\s+", "-", RegexOptions.Compiled);

        // Remove invalid characters
        text = Regex.Replace(text, @"[^a-z0-9\s\-]", "", RegexOptions.Compiled);

        // Replace multiple hyphens with single hyphen
        text = Regex.Replace(text, @"\-+", "-", RegexOptions.Compiled);

        // Trim hyphens from ends
        text = text.Trim('-');

        return text;
    }
}


