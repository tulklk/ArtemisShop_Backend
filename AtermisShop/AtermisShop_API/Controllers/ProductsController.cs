using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Application.Products.Queries.GetFeaturedProducts;
using AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;
using AtermisShop.Application.Products.Queries.GetProducts;
using AtermisShop.Application.Products.Queries.SearchProducts;
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProductCommand(
            request.Name,
            request.Slug,
            request.Description,
            request.Price,
            request.CategoryId), cancellationToken);

        return Ok(new { id });
    }
}

public sealed class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
}


