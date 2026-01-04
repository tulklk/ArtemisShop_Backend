using AtermisShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace AtermisShop.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler>? _logger;

    public ForgotPasswordCommandHandler(
        IUserService userService,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler>? logger = null)
    {
        _userService = userService;
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
            // Generate new 8-digit password
            var newPassword = GenerateRandomPassword(8);

            // Reset password in database
            var passwordResetSuccess = await _userService.ResetPasswordAsync(user.Id, newPassword);
            if (!passwordResetSuccess)
            {
                _logger?.LogError("Failed to reset password for user {UserId}", user.Id);
                return true; // Still return true to not reveal if user exists
            }

            // Send email with new password
            await _emailService.SendNewPasswordAsync(
                user.Email,
                user.FullName ?? user.Email,
                newPassword,
                cancellationToken);

            _logger?.LogInformation("New password sent successfully to {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send new password email to {Email}", user.Email);
            // Still return true to not reveal if user exists
            return true;
        }
    }

    private static string GenerateRandomPassword(int length)
    {
        const string digits = "0123456789";
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        
        var password = new char[length];
        for (int i = 0; i < length; i++)
        {
            password[i] = digits[bytes[i] % digits.Length];
        }
        
        return new string(password);
    }
}

