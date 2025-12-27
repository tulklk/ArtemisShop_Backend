using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Application.Auth.Commands.ResendVerification;

public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, bool>
{
    private readonly IUserService _userService;
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendVerificationCommandHandler>? _logger;

    public ResendVerificationCommandHandler(
        IUserService userService,
        IEmailVerificationTokenService tokenService,
        IEmailService emailService,
        ILogger<ResendVerificationCommandHandler>? logger = null)
    {
        _userService = userService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user == null || user.EmailVerified)
            return false;

        try
        {
            // Generate new verification token
            var token = await _tokenService.GenerateTokenAsync(user.Id, cancellationToken);
            
            // Send verification email
            await _emailService.SendEmailVerificationAsync(
                user.Email,
                user.FullName ?? user.Email,
                token,
                cancellationToken);
            
            _logger?.LogInformation("Verification email resent to {Email} for user {UserId}", user.Email, user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to resend verification email to {Email}", request.Email);
            return false;
        }
    }
}

