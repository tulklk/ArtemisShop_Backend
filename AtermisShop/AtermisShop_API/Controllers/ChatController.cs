using AtermisShop.Application.Chat.Commands.ChatWithGemini;
using AtermisShop.Application.Chat.Common;
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
    /// <returns>AI response with suggested products</returns>
    /// <response code="200">Returns AI response with suggested products</response>
    [HttpPost("message")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        try
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
            
            return Ok(new ChatResponseDto
            {
                Message = result.Response,
                Success = true,
                Error = null,
                SuggestedProducts = result.SuggestedProducts
            });
        }
        catch (Exception ex)
        {
            return Ok(new ChatResponseDto
            {
                Message = "Xin lỗi, tôi gặp lỗi khi xử lý câu hỏi của bạn. Vui lòng thử lại sau.",
                Success = false,
                Error = ex.Message,
                SuggestedProducts = new List<SuggestedProductDto>()
            });
        }
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

