using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IUserService _userService;
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterCommandHandler>? _logger;

    public RegisterCommandHandler(
        IUserService userService,
        IEmailVerificationTokenService tokenService,
        IEmailService emailService,
        ILogger<RegisterCommandHandler>? logger = null)
    {
        _userService = userService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userService.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            FullName = request.FullName ?? string.Empty,
            EmailVerified = false,
            Role = 0, // Default role
            IsActive = true
        };

        await _userService.CreateAsync(user, request.Password);

        // Generate verification token and send email
        try
        {
            var token = await _tokenService.GenerateTokenAsync(user.Id, cancellationToken);
            await _emailService.SendEmailVerificationAsync(
                user.Email,
                user.FullName ?? user.Email,
                token,
                cancellationToken);
            
            _logger?.LogInformation("Verification email sent to {Email} for user {UserId}", user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            // Don't throw - user is created, they can request resend
        }

        return user.Id;
    }
}


