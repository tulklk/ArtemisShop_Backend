using AtermisShop.Application.News.Commands.CreateNews;
using AtermisShop.Application.News.Commands.DeleteNews;
using AtermisShop.Application.News.Commands.UpdateNews;
using AtermisShop.Application.News.Queries.GetAllNews;
using AtermisShop.Application.News.Queries.GetNewsById;
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
        var news = await _mediator.Send(new GetAllNewsQuery(), cancellationToken);
        return Ok(news);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNews([FromBody] CreateNewsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid? authorId = null;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                authorId = parsedUserId;
            }

            var result = await _mediator.Send(new CreateNewsCommand(
                request.Title,
                request.Content,
                request.Summary,
                request.ThumbnailUrl,
                request.Category,
                request.Tags,
                request.IsPublished,
                authorId), cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNews(Guid id, CancellationToken cancellationToken)
    {
        var news = await _mediator.Send(new GetNewsByIdQuery(id), cancellationToken);
        if (news == null)
            return NotFound();
        return Ok(news);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNews(Guid id, [FromBody] UpdateNewsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new UpdateNewsCommand(
                id,
                request.Title,
                request.Content,
                request.Summary,
                request.ThumbnailUrl,
                request.Category,
                request.Tags,
                request.IsPublished), cancellationToken);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNews(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DeleteNewsCommand(id), cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public record CreateNewsRequest(
        string Title,
        string Content,
        string? Summary,
        string? ThumbnailUrl,
        string? Category,
        string? Tags,
        bool IsPublished);

    public record UpdateNewsRequest(
        string Title,
        string Content,
        string? Summary,
        string? ThumbnailUrl,
        string? Category,
        string? Tags,
        bool IsPublished);
}

