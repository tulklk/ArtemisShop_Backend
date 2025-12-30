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
            request.ImageUrls,
            request.Variants), cancellationToken);

        return Ok(new { id });
    }
}

public sealed class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; }
    public List<string>? ImageUrls { get; set; }
    public List<ProductVariantDto>? Variants { get; set; }
}

public sealed record ValidateEngravingTextRequest(string? EngravingText);


