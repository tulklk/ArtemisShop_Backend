using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminNewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminNewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews(CancellationToken cancellationToken)
    {
        // TODO: Implement get all news query
        return Ok(new List<object>());
    }

    [HttpPost]
    public async Task<IActionResult> CreateNews([FromBody] CreateNewsRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement create news command
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNews(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement get news by id query
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNews(Guid id, [FromBody] UpdateNewsRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement update news command
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNews(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement delete news command
        return NoContent();
    }

    public record CreateNewsRequest(string Title, string Slug, string Content, string? Excerpt, string? ImageUrl, Guid? CategoryId, bool IsPublished);
    public record UpdateNewsRequest(string Title, string Slug, string Content, string? Excerpt, string? ImageUrl, Guid? CategoryId, bool IsPublished);
}

