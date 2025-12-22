using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AtermisShop.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<JwtTokenResult> GenerateTokensAsync(ApplicationUser user)
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
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

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

        return Task.FromResult(new JwtTokenResult
        {
            AccessToken = handler.WriteToken(token),
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = now.AddDays(refreshDays)
        });
    }

    public JwtTokenResult GenerateTokens(ApplicationUser user)
    {
        return GenerateTokensAsync(user).Result;
    }
}


