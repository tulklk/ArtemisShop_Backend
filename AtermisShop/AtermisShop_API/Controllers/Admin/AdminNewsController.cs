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
                request.NewsUrl,
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

    /// <summary>
    /// Upload image for news content
    /// </summary>
    [HttpPost("upload-image")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        // Validate file extension - allow common image formats
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".JPG", ".JPEG", ".PNG", ".GIF", ".WEBP" };
        var fileExtension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { error = "Only image files are allowed (jpg, jpeg, png, gif, webp)" });

        // Validate file size (max 10MB)
        const long maxFileSize = 10_000_000; // 10MB
        if (file.Length > maxFileSize)
            return BadRequest(new { error = "File size exceeds 10MB limit" });

        try
        {
            // Save to volume directory (/data/uploads/news)
            var savePath = Path.Combine("/data", "uploads", "news");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Console.WriteLine($"Created news uploads directory: {savePath}");
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
            Console.WriteLine($"News image uploaded successfully: {fileName}, Size: {fileInfo.Length} bytes, Path: {filePath}");

            // Return URL path - this URL can be used in HTML content
            var fileUrl = $"/uploads/news/{fileName}";
            return Ok(new { url = fileUrl, fileName = fileName, size = fileInfo.Length });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading news image: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = $"Failed to upload file: {ex.Message}" });
        }
    }

    public record CreateNewsRequest(
        string Title,
        string Content,
        string? Summary,
        string? ThumbnailUrl,
        string? Category,
        string? Tags,
        bool IsPublished,
        string? NewsUrl);

    public record UpdateNewsRequest(
        string Title,
        string Content,
        string? Summary,
        string? ThumbnailUrl,
        string? Category,
        string? Tags,
        bool IsPublished);
}

