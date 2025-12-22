using AtermisShop.Application.Products.Commands.CreateProduct;
using AtermisShop.Application.Products.Queries.GetProductByIdOrSlug;
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

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProductCommand(
            request.Name, request.Slug, request.Description, request.Price, request.CategoryId), cancellationToken);
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
        // TODO: Implement update command
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete command
        return NoContent();
    }

    public record CreateProductRequest(string Name, string Slug, string? Description, decimal Price, Guid CategoryId);
    public record UpdateProductRequest(string Name, string Slug, string? Description, decimal Price, Guid CategoryId);
}

