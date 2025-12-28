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
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserService userService,
        IEmailVerificationTokenService tokenService,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate password confirmation
            if (request.Password != request.ConfirmPassword)
            {
                throw new InvalidOperationException("Mật khẩu và xác nhận mật khẩu không khớp.");
            }

            // Check if user already exists
            var existingUser = await _userService.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email này đã được sử dụng. Vui lòng sử dụng email khác.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailVerified = false,
                Role = 0, // Default role
                IsActive = true
            };

            _logger.LogInformation("Creating user with email: {Email}", request.Email);
            await _userService.CreateAsync(user, request.Password);
            _logger.LogInformation("User created successfully with ID: {UserId}, Email: {Email}", user.Id, user.Email);

            // Generate verification token and send email immediately after registration
            try
            {
                _logger.LogInformation("Generating verification token for user {UserId} ({Email})", user.Id, user.Email);
                
                var token = await _tokenService.GenerateTokenAsync(user.Id, cancellationToken);
                
                _logger.LogInformation("Sending verification email to {Email}", user.Email);
                
                await _emailService.SendEmailVerificationAsync(
                    user.Email,
                    user.FullName ?? user.Email,
                    token,
                    cancellationToken);
                
                _logger.LogInformation("Verification email sent successfully to {Email} for user {UserId}", user.Email, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}. Error: {ErrorMessage}", user.Email, ex.Message);
                // Don't throw - user is created successfully, they can request resend later
                // The registration is still considered successful even if email fails
            }

            return user.Id;
        }
        catch (InvalidOperationException)
        {
            // Re-throw InvalidOperationException (like duplicate email) to be handled by controller
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RegisterCommandHandler for email: {Email}. Error: {ErrorMessage}", request.Email, ex.Message);
            throw;
        }
    }
}


