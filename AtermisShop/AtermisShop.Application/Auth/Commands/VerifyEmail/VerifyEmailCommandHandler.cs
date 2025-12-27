using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly IUserService _userService;
    private readonly ILogger<VerifyEmailCommandHandler>? _logger;

    public VerifyEmailCommandHandler(
        IEmailVerificationTokenService tokenService,
        IUserService userService,
        ILogger<VerifyEmailCommandHandler>? logger = null)
    {
        _tokenService = tokenService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // Validate token
        var verificationToken = await _tokenService.ValidateTokenAsync(request.Token, cancellationToken);
        if (verificationToken == null)
        {
            _logger?.LogWarning("Invalid or expired email verification token");
            return false;
        }

        var user = verificationToken.User;
        if (user == null)
        {
            _logger?.LogWarning("User not found for verification token");
            return false;
        }

        if (user.EmailVerified)
        {
            _logger?.LogInformation("Email already verified for user {UserId}", user.Id);
            // Still invalidate the token even if already verified
            await _tokenService.InvalidateTokenAsync(request.Token, cancellationToken);
            return true;
        }

        // Mark email as verified
        user.EmailVerified = true;
        await _userService.UpdateAsync(user);

        // Invalidate the token so it can't be reused
        await _tokenService.InvalidateTokenAsync(request.Token, cancellationToken);

        _logger?.LogInformation("Email verified successfully for user {UserId} ({Email})", user.Id, user.Email);

        return true;
    }
}

