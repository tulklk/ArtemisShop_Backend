using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AtermisShop.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IConfiguration configuration,
        ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<JwtTokenResult> GenerateTokensAsync(ApplicationUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;
        var secret = jwtSection["Secret"]!;
        var accessMinutes = int.Parse(jwtSection["AccessTokenMinutes"] ?? "60");
        var refreshDays = int.Parse(jwtSection["RefreshTokenDays"] ?? "30");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email)
        };

        // Add role claim based on Role field (1 = Admin)
        if (user.Role == 1)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            _logger.LogInformation("User {UserId} ({Email}) is Admin", user.Id, user.Email);
        }
        else
        {
            _logger.LogInformation("User {UserId} ({Email}) has role: {Role}", user.Id, user.Email, user.Role);
        }

        var now = DateTime.UtcNow;
        var accessExpires = now.AddMinutes(accessMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            now,
            accessExpires,
            creds);

        var handler = new JwtSecurityTokenHandler();

        // Simplified refresh token for now
        var refreshToken = Guid.NewGuid().ToString("N");

        return new JwtTokenResult
        {
            AccessToken = handler.WriteToken(token),
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = now.AddDays(refreshDays)
        };
    }

    public JwtTokenResult GenerateTokens(ApplicationUser user)
    {
        return GenerateTokensAsync(user).Result;
    }
}


