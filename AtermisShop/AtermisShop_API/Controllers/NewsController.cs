using AtermisShop.Application.News.Queries.GetNews;
using AtermisShop.Application.News.Queries.GetNewsByIdOrSlug;
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

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetNews(CancellationToken cancellationToken)
    {
        var news = await _mediator.Send(new GetNewsQuery(), cancellationToken);
        return Ok(news);
    }

    [HttpGet("{idOrSlug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNewsByIdOrSlug(string idOrSlug, CancellationToken cancellationToken)
    {
        var article = await _mediator.Send(new GetNewsByIdOrSlugQuery(idOrSlug), cancellationToken);
        if (article == null)
            return NotFound();
        return Ok(article);
    }

    // NewsCategories removed - using Category field on NewsPost instead
}

