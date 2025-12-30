using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Application.Products.Commands.DeleteProduct;
using AtermisShop.Application.Products.Commands.UpdateProduct;
using AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;
using AtermisShop.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
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
            request.ImageUrls,
            request.Variants), cancellationToken);
        return Ok(new { Id = id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductByIdOrSlugQuery(id.ToString()), cancellationToken);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new UpdateProductCommand(
                id,
                request.Name,
                request.Slug,
                request.Description,
                request.Price,
                request.CategoryId,
                request.OriginalPrice,
                request.StockQuantity,
                request.Brand,
                request.IsActive,
                request.HasEngraving,
                request.DefaultEngravingText), cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public record CreateProductRequest(
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
        List<ProductVariantDto>? Variants);
    public record UpdateProductRequest(
        string Name, 
        string Slug, 
        string? Description, 
        decimal Price, 
        Guid CategoryId,
        decimal? OriginalPrice = null,
        int? StockQuantity = null,
        string? Brand = null,
        bool? IsActive = null,
        bool? HasEngraving = null,
        string? DefaultEngravingText = null);
}

