using AtermisShop.Application.Categories.Commands.CreateCategory;
using AtermisShop.Application.Categories.Commands.UpdateCategory;
using AtermisShop.Application.Categories.Queries.GetCategories;
using AtermisShop.Application.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(
            request.Name, 
            request.Description, 
            request.Children), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (category == null)
            return NotFound();
        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.Children), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete command
        return NoContent();
    }

    public record CreateCategoryRequest(string Name, string? Description, List<string>? Children);
    public record UpdateCategoryRequest(string Name, string? Description, List<AtermisShop.Application.Categories.Commands.UpdateCategory.ChildCategoryDto>? Children);
}

