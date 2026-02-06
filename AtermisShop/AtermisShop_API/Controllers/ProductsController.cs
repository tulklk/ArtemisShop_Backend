using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Application.Products.Common;
using AtermisShop.Application.Products.Queries.GetEngravingInfo;
using AtermisShop.Application.Products.Queries.GetFeaturedProducts;
using AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;
using AtermisShop.Application.Products.Queries.GetProductStatistics;
using AtermisShop.Application.Products.Queries.GetProducts;
using AtermisShop.Application.Products.Queries.SearchProducts;
using AtermisShop.Application.Products.Queries.ValidateEngravingText;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(products);
    }

    [HttpGet("{idOrSlug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdOrSlug(string idOrSlug, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductByIdOrSlugQuery(idOrSlug), cancellationToken);
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured(CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(new GetFeaturedProductsQuery(), cancellationToken);
        return Ok(products);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(new SearchProductsQuery(keyword), cancellationToken);
        return Ok(products);
    }

    [HttpGet("{productId}/statistics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatistics(Guid productId, CancellationToken cancellationToken)
    {
        var statistics = await _mediator.Send(new GetProductStatisticsQuery(productId), cancellationToken);
        if (statistics is null) return NotFound();
        return Ok(statistics);
    }

    /// <summary>
    /// Get engraving information (rules, max length, etc.)
    /// </summary>
    [HttpGet("engraving/info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EngravingInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEngravingInfo(CancellationToken cancellationToken)
    {
        var info = await _mediator.Send(new GetEngravingInfoQuery(), cancellationToken);
        return Ok(info);
    }

    /// <summary>
    /// Validate engraving text
    /// </summary>
    [HttpPost("engraving/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidateEngravingTextResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateEngravingText([FromBody] ValidateEngravingTextRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ValidateEngravingTextQuery(request.EngravingText), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProductCommand(
            request.Name,
            request.Description,
            request.CategoryId,
            request.Price,
            request.OriginalPrice,
            request.StockQuantity,
            request.Brand,
            request.IsActive,
            request.HasEngraving,
            request.DefaultEngravingText,
            request.Model3DUrl,
            request.Images,
            request.Variants), cancellationToken);

        return Ok(new { id });
    }

    /// <summary>
    /// Upload 3D model file (GLB format) for a product
    /// </summary>
    [HttpPost("upload-model3d")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(50_000_000)] // 50MB limit
    public async Task<IActionResult> UploadModel3D(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        // Validate file extension
        var allowedExtensions = new[] { ".glb", ".GLB" };
        var fileExtension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { error = "Only GLB files are allowed" });

        // Validate file size (max 50MB)
        const long maxFileSize = 50_000_000; // 50MB
        if (file.Length > maxFileSize)
            return BadRequest(new { error = "File size exceeds 50MB limit" });

        try
        {
            // Save to volume directory (/data/uploads/models3d)
            var savePath = Path.Combine("/data", "uploads", "models3d");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Console.WriteLine($"Created uploads directory on volume: {savePath}");
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(savePath, fileName);

            // Save file
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Verify file was saved
            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(500, new { error = "File was not saved successfully" });
            }

            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"File uploaded successfully: {fileName}, Size: {fileInfo.Length} bytes, Path: {filePath}");

            // Return URL path
            var fileUrl = $"/uploads/models3d/{fileName}";
            return Ok(new { url = fileUrl, fileName = fileName, size = fileInfo.Length });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = $"Failed to upload file: {ex.Message}" });
        }
    }
}

public sealed class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; }
    public bool HasEngraving { get; set; }
    public string? DefaultEngravingText { get; set; }
    public string? Model3DUrl { get; set; }
    public List<CreateProductImageDto>? Images { get; set; }
    public List<ProductVariantDto>? Variants { get; set; }
}

public sealed record ValidateEngravingTextRequest(string? EngravingText);


