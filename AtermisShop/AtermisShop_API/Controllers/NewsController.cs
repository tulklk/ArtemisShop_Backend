using AtermisShop.Application.News.Queries.GetNews;
using AtermisShop.Application.News.Queries.GetNewsByIdOrSlug;
using AtermisShop.Application.News.Queries.GetChildAbductionNews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all published news articles
    /// </summary>
    /// <param name="page">Page number (optional)</param>
    /// <param name="pageSize">Number of items per page (optional)</param>
    /// <param name="search">Search by title, summary, or content (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of news articles</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetNews(
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var news = await _mediator.Send(new GetNewsQuery(page, pageSize, search), cancellationToken);
        return Ok(news);
    }

    /// <summary>
    /// Get a news article by ID or slug
    /// </summary>
    /// <param name="idOrSlug">News ID (Guid) or slug</param>
    /// <param name="incrementViewCount">Whether to increment view count (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News article details</returns>
    [HttpGet("{idOrSlug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNewsByIdOrSlug(
        string idOrSlug,
        [FromQuery] bool incrementViewCount = true,
        CancellationToken cancellationToken = default)
    {
        var article = await _mediator.Send(new GetNewsByIdOrSlugQuery(idOrSlug, incrementViewCount), cancellationToken);
        if (article == null)
            return NotFound(new { message = "News article not found" });
        return Ok(article);
    }

    /// <summary>
    /// Get child abduction related news from multiple RSS sources with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 6)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of child abduction related articles from all sources</returns>
    [HttpGet("child-abduction")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChildAbductionNews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChildAbductionNewsQuery 
        { 
            Page = page, 
            PageSize = pageSize
        };
        
        var articles = await _mediator.Send(query, cancellationToken);
        
        var totalCount = articles.FirstOrDefault()?.TotalArticlesFetched ?? 0;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return Ok(new 
        { 
            page = page,
            pageSize = pageSize,
            totalCount = totalCount,
            totalPages = totalPages,
            sources = new[] { "VnExpress", "Tuổi Trẻ", "Thanh Niên", "Dân Trí" },
            articles = articles
        });
    }

    // NewsCategories removed - using Category field on NewsPost instead
}

