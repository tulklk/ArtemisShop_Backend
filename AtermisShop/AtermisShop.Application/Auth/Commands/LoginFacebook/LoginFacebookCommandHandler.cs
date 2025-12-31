using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AtermisShop.Application.Auth.Commands.LoginFacebook;

public sealed class LoginFacebookCommandHandler : IRequestHandler<LoginFacebookCommand, JwtTokenResult?>
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginFacebookCommandHandler> _logger;
    private readonly HttpClient _httpClient;

    public LoginFacebookCommandHandler(
        IUserService userService,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService,
        ILogger<LoginFacebookCommandHandler> logger,
        IHttpClientFactory httpClientFactory)
    {
        _userService = userService;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<JwtTokenResult?> Handle(LoginFacebookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
            {
                _logger.LogWarning("Facebook access token is empty");
                return null;
            }

            // Get Facebook App ID and Secret from configuration
            var facebookAppId = _configuration["FacebookOAuth:AppId"];
            var facebookAppSecret = _configuration["FacebookOAuth:AppSecret"];
            
            if (string.IsNullOrWhiteSpace(facebookAppId) || string.IsNullOrWhiteSpace(facebookAppSecret))
            {
                _logger.LogError("Facebook OAuth is not properly configured");
                throw new InvalidOperationException("Facebook OAuth is not properly configured. Please set FacebookOAuth:AppId and FacebookOAuth:AppSecret in appsettings.json");
            }

            // Validate Facebook access token and get user info
            FacebookUserInfo? userInfo;
            try
            {
                var url = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={request.AccessToken}";
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Facebook Graph API returned error: {StatusCode}", response.StatusCode);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error calling Facebook Graph API");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error parsing Facebook Graph API response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating Facebook access token");
                return null;
            }

            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogWarning("Facebook user info is missing email");
                return null;
            }

            // Find user by email
            var user = await _userService.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                // Create new user from Facebook account
                user = new ApplicationUser
                {
                    Email = userInfo.Email,
                    EmailVerified = true,
                    FullName = userInfo.Name ?? userInfo.Email.Split('@')[0],
                    Avatar = userInfo.Picture?.Data?.Url,
                    FacebookId = userInfo.Id,
                    Role = 0, // Default role
                    IsActive = true
                };

                // Create user without password (Facebook auth)
                await _userService.CreateAsync(user, Guid.NewGuid().ToString()); // Temporary password for Facebook users
                
                _logger.LogInformation("Created new user from Facebook login: {Email}", userInfo.Email);
            }
            else
            {
                // Update user info if needed
                var shouldUpdate = false;
                
                // Update FacebookId if not set
                if (string.IsNullOrEmpty(user.FacebookId) && !string.IsNullOrEmpty(userInfo.Id))
                {
                    user.FacebookId = userInfo.Id;
                    shouldUpdate = true;
                }
                
                // Update avatar if not set or different
                var pictureUrl = userInfo.Picture?.Data?.Url;
                if ((string.IsNullOrEmpty(user.Avatar) || user.Avatar != pictureUrl) && !string.IsNullOrEmpty(pictureUrl))
                {
                    user.Avatar = pictureUrl;
                    shouldUpdate = true;
                }
                
                // Update full name if not set
                if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(userInfo.Name))
                {
                    user.FullName = userInfo.Name;
                    shouldUpdate = true;
                }
                
                // Mark email as verified for Facebook users
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
            var tokens = await _jwtTokenService.GenerateTokensAsync(user);
            
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
            _logger.LogError(ex, "Unexpected error during Facebook login");
            return null;
        }
    }

    // Helper classes for Facebook API response
    private class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        public string Url { get; set; } = string.Empty;
    }
}

