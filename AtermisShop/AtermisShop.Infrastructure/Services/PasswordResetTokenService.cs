using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace AtermisShop.Infrastructure.Services;

public class PasswordResetTokenService : IPasswordResetTokenService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PasswordResetTokenService> _logger;
    private const int TokenExpirationHours = 1; // Password reset tokens expire in 1 hour

    public PasswordResetTokenService(
        IApplicationDbContext context,
        ILogger<PasswordResetTokenService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Generate a secure random token
        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        // Convert to base64 URL-safe string
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        // Invalidate any existing unused tokens for this user
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var existingToken in existingTokens)
        {
            existingToken.IsUsed = true;
        }

        // Create new token
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(TokenExpirationHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated password reset token for user {UserId}. Expires at {ExpiresAt}", userId, resetToken.ExpiresAt);

        return token;
    }

    public async Task<PasswordResetToken?> ValidateTokenAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (resetToken == null)
        {
            _logger.LogWarning("Password reset token not found: {Token}", token);
            return null;
        }

        if (resetToken.IsUsed)
        {
            _logger.LogWarning("Password reset token already used: {Token}", token);
            return null;
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset token expired: {Token}, Expired at {ExpiresAt}", token, resetToken.ExpiresAt);
            return null;
        }

        return resetToken;
    }

    public async Task InvalidateTokenAsync(string token, CancellationToken cancellationToken)
    {
        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (resetToken != null)
        {
            resetToken.IsUsed = true;
            resetToken.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);
            _logger.LogInformation("Invalidated password reset token: {Token}", token);
        }
    }
}

