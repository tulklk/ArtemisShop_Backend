using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Application.Auth.Commands.LoginGoogle;

public sealed class LoginGoogleCommandHandler : IRequestHandler<LoginGoogleCommand, JwtTokenResult?>
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginGoogleCommandHandler> _logger;

    public LoginGoogleCommandHandler(
        IUserService userService,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService,
        ILogger<LoginGoogleCommandHandler> logger)
    {
        _userService = userService;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<JwtTokenResult?> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                _logger.LogWarning("Google ID token is empty");
                return null;
            }

            // Get Google Client ID from configuration
            var googleClientId = _configuration["GoogleOAuth:ClientId"];
            if (string.IsNullOrWhiteSpace(googleClientId) || googleClientId == "YOUR_GOOGLE_CLIENT_ID_HERE")
            {
                _logger.LogError("Google OAuth Client ID is not configured");
                throw new InvalidOperationException("Google OAuth is not properly configured. Please set GoogleOAuth:ClientId in appsettings.json");
            }

            // Validate Google ID token
            GoogleJsonWebSignature.Payload? payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google ID token");
                return null;
            }

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                _logger.LogWarning("Google token payload is missing email");
                return null;
            }

            // Find user by email
            var user = await _userService.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create new user from Google account
                user = new ApplicationUser
                {
                    Email = payload.Email,
                    EmailVerified = true,
                    FullName = payload.Name ?? payload.Email.Split('@')[0],
                    Avatar = payload.Picture,
                    GoogleId = payload.Subject,
                    Role = 0, // Default role
                    IsActive = true
                };

                // Create user without password (Google auth)
                await _userService.CreateAsync(user, Guid.NewGuid().ToString()); // Temporary password for Google users
                
                _logger.LogInformation("Created new user from Google login: {Email}", payload.Email);
            }
            else
            {
                // Update user info if needed
                var shouldUpdate = false;
                
                // Update GoogleId if not set
                if (string.IsNullOrEmpty(user.GoogleId) && !string.IsNullOrEmpty(payload.Subject))
                {
                    user.GoogleId = payload.Subject;
                    shouldUpdate = true;
                }
                
                // Update avatar if not set or different
                if ((string.IsNullOrEmpty(user.Avatar) || user.Avatar != payload.Picture) && !string.IsNullOrEmpty(payload.Picture))
                {
                    user.Avatar = payload.Picture;
                    shouldUpdate = true;
                }
                
                // Update full name if not set
                if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(payload.Name))
                {
                    user.FullName = payload.Name;
                    shouldUpdate = true;
                }
                
                // Mark email as verified for Google users
                if (!user.EmailVerified)
                {
                    user.EmailVerified = true;
                    shouldUpdate = true;
                }

                if (shouldUpdate)
                {
                    await _userService.UpdateAsync(user);
                }
            }

            // Generate JWT tokens
            var tokens = await _jwtTokenService.GenerateTokensAsync(user, cancellationToken);
            
            // Add user information to response
            tokens.User = new Common.UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Role = user.Role,
                EmailVerified = user.EmailVerified
            };

            return tokens;
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw configuration errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Google login");
            return null;
        }
    }
}

