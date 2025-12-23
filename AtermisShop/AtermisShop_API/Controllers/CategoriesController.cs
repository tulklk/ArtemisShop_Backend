using AtermisShop.Application.Categories.Commands.CreateCategory;
using AtermisShop.Application.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(
            request.Name, 
            request.Description, 
            request.Children), cancellationToken);
        return Ok(result);
    }
}

public sealed class CreateCategoryRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<string>? Children { get; set; }
}


