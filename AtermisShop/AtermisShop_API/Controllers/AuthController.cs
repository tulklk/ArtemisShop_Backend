using AtermisShop.Application.Auth.Commands.ForgotPassword;
using AtermisShop.Application.Auth.Commands.Login;
using AtermisShop.Application.Auth.Commands.LoginFacebook;
using AtermisShop.Application.Auth.Commands.LoginGoogle;
using AtermisShop.Application.Auth.Commands.RefreshToken;
using AtermisShop.Application.Auth.Commands.Register;
using AtermisShop.Application.Auth.Commands.ResendVerification;
using AtermisShop.Application.Auth.Commands.VerifyEmail;
using AtermisShop.Application.Auth.Queries.GetMe;
using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AtermisShop_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserService _userService;
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IUserService userService,
        IEmailVerificationTokenService tokenService,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _userService = userService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Dữ liệu không hợp lệ." });
        }

        if (request == null)
        {
            return BadRequest(new { message = "Request body không được để trống." });
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email là bắt buộc." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Mật khẩu là bắt buộc." });
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return BadRequest(new { message = "Xác nhận mật khẩu là bắt buộc." });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Họ tên là bắt buộc." });
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(new { message = "Số điện thoại là bắt buộc." });
        }

        try
        {
            var userId = await _mediator.Send(new RegisterCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.FullName,
                request.PhoneNumber
            ), cancellationToken);
            
            return Ok(new { message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request?.Email ?? "unknown");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại sau." });
        }
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

    [HttpPost("login/facebook")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginFacebook([FromBody] LoginFacebookRequest request, CancellationToken cancellationToken)
    {
        var tokens = await _mediator.Send(new LoginFacebookCommand(request.AccessToken), cancellationToken);
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
        try
        {
            var result = await _mediator.Send(new VerifyEmailCommand(request.Token), cancellationToken);
            if (!result)
                return BadRequest(new { success = false, message = "Xác thực không thành công." });
                
            return Ok(new { success = true, message = "Xác thực email thành công!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for token {Token}", request?.Token);
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi trong quá trình xác thực email." });
        }
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new ResendVerificationCommand(request.Email), cancellationToken);
            return Ok(new { success = result, message = "Email xác thực đã được gửi lại." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resending verification for email: {Email}", request.Email);
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi gửi lại email xác thực." });
        }
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
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { success = false, message = "Email is required" });
        }

        // Find user by email
        var user = await _userService.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        try
        {
            // Generate verification token
            var token = await _tokenService.GenerateTokenAsync(user.Id, cancellationToken);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(
                user.Email,
                user.FullName ?? user.Email,
                token,
                cancellationToken);

            return Ok(new 
            { 
                success = true, 
                message = "Test verification email sent successfully",
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                success = false, 
                message = "Failed to send test email",
                error = ex.Message
            });
        }
    }
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

public sealed class LoginFacebookRequest
{
    public string AccessToken { get; set; } = default!;
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}

public sealed class VerifyEmailRequest
{
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

public sealed class TestEmailRequest
{
    public string Email { get; set; } = default!;
}

public sealed class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}


