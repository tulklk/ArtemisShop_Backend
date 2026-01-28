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
        _logger?.LogInformation("Attempting to verify email with token: {Token}", request.Token);
        
        // Validate token
        var verificationToken = await _tokenService.ValidateTokenAsync(request.Token, cancellationToken);
        if (verificationToken == null)
        {
            // We need to know WHY it's null. Since ValidateTokenAsync returns null for all failures, 
            // let's re-verify here or trust the logs. 
            // Better: improve ValidateTokenAsync or handle here.
            // For now, let's provide a generic but clear message that points to potential issues.
            throw new InvalidOperationException("Mã xác thực không hợp lệ, đã được sử dụng hoặc đã hết hạn.");
        }

        var user = verificationToken.User;
        if (user == null)
        {
            throw new InvalidOperationException("Không tìm thấy người dùng gắn liền với mã xác thực này.");
        }

        if (user.EmailVerified)
        {
            _logger?.LogInformation("Email already verified for user {UserId} ({Email})", user.Id, user.Email);
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

