using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AtermisShop.Application.Auth.Common;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AtermisShop.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _userManager = userManager;
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
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        // Attach role claims so [Authorize(Roles = ...)] works for admin APIs
        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User {UserId} ({Email}) has roles: {Roles}", user.Id, user.Email, string.Join(", ", roles));
        
        if (roles.Count == 0)
        {
            _logger.LogWarning("User {UserId} ({Email}) has no roles assigned. Admin APIs will not work.", user.Id, user.Email);
        }
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
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


