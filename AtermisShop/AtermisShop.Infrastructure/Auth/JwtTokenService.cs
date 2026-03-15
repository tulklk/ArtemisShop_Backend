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
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<JwtTokenService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<JwtTokenResult> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
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
        var refreshTokenString = Guid.NewGuid().ToString("N");
        var refreshTokenExpiresAt = now.AddDays(refreshDays);

        // Save refresh token to database - must succeed so refresh works later
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = now
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        try
        {
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Saved refresh token to database for user {UserId}. Result: {Result}", user.Id, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh token to database for user {UserId}. Login will fail so client does not receive an unusable token.", user.Id);
            throw;
        }

        return new JwtTokenResult
        {
            AccessToken = handler.WriteToken(token),
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    public JwtTokenResult GenerateTokens(ApplicationUser user)
    {
        return GenerateTokensAsync(user, CancellationToken.None).Result;
    }
}


