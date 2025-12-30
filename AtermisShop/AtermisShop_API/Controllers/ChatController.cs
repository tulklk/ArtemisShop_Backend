using AtermisShop.Application.Chat.Commands.ChatWithGemini;
using AtermisShop.Application.Chat.Commands.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Chat with AI assistant (Gemini)
    /// </summary>
    /// <param name="request">Chat message request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI response</returns>
    /// <response code="200">Returns AI response</response>
    [HttpPost("message")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        var result = await _mediator.Send(new ChatWithGeminiCommand(
            request.Message, 
            request.SessionId, 
            userId), cancellationToken);
        
        return Ok(new ChatResponse
        {
            Response = result.Response,
            SessionId = result.SessionId
        });
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy" });
    }

    [HttpPost("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new { message = "Chat test endpoint" });
    }

    public record SendMessageRequest(string Message, string? SessionId);
    
    public class ChatResponse
    {
        public string Response { get; set; } = default!;
        public string SessionId { get; set; } = default!;
    }
}

