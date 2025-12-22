using AtermisShop.Application.Auth.Commands.ForgotPassword;
using AtermisShop.Application.Auth.Commands.Login;
using AtermisShop.Application.Auth.Commands.LoginGoogle;
using AtermisShop.Application.Auth.Commands.RefreshToken;
using AtermisShop.Application.Auth.Commands.Register;
using AtermisShop.Application.Auth.Commands.ResendVerification;
using AtermisShop.Application.Auth.Commands.VerifyEmail;
using AtermisShop.Application.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.FullName), cancellationToken);
        return Ok(new { id });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var tokens = await _mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(tokens);
    }

    [HttpPost("login/google")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginGoogle([FromBody] LoginGoogleRequest request, CancellationToken cancellationToken)
    {
        var tokens = await _mediator.Send(new LoginGoogleCommand(request.IdToken), cancellationToken);
        if (tokens == null)
            return Unauthorized();
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokens = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
        if (tokens == null)
            return Unauthorized();
        return Ok(tokens);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _mediator.Send(new GetMeQuery(userId), cancellationToken);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // In production, invalidate refresh token in DB
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(request.UserId, request.Token), cancellationToken);
        return Ok(new { success = result });
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResendVerificationCommand(request.Email), cancellationToken);
        return Ok(new { success = result });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Ok(new { success = result });
    }

    [HttpPost("test-email")]
    [AllowAnonymous]
    public IActionResult TestEmail()
    {
        // TODO: Implement test email sending
        return Ok(new { message = "Email test endpoint - not implemented yet" });
    }
}

public sealed class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? FullName { get; set; }
}

public sealed class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class LoginGoogleRequest
{
    public string IdToken { get; set; } = default!;
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}

public sealed class VerifyEmailRequest
{
    public string UserId { get; set; } = default!;
    public string Token { get; set; } = default!;
}

public sealed class ResendVerificationRequest
{
    public string Email { get; set; } = default!;
}

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}


