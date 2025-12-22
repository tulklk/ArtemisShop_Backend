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

    [HttpPost("message")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        var messageId = await _mediator.Send(new SendMessageCommand(userId, request.Message, request.SessionId), cancellationToken);
        return Ok(new { MessageId = messageId });
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
}

