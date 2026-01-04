using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserService _userService;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler>? _logger;

    public ForgotPasswordCommandHandler(
        IUserService userService,
        IPasswordResetTokenService passwordResetTokenService,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler>? logger = null)
    {
        _userService = userService;
        _passwordResetTokenService = passwordResetTokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists - return true anyway for security
            _logger?.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
            return true;
        }

        try
        {
            // Generate password reset token
            var resetToken = await _passwordResetTokenService.GenerateTokenAsync(user.Id, cancellationToken);

            // Send password reset email
            await _emailService.SendPasswordResetAsync(
                user.Email,
                user.FullName ?? user.Email,
                resetToken,
                cancellationToken);

            _logger?.LogInformation("Password reset email sent successfully to {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            // Still return true to not reveal if user exists
            return true;
        }
    }
}

