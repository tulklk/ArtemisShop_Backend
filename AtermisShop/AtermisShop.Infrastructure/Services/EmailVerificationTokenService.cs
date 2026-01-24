using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AtermisShop.Infrastructure.Services;

public class EmailVerificationTokenService : IEmailVerificationTokenService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EmailVerificationTokenService> _logger;
    private const int TokenExpirationHours = 24;

    public EmailVerificationTokenService(
        IApplicationDbContext context,
        ILogger<EmailVerificationTokenService> logger)
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
        var existingTokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var existingToken in existingTokens)
        {
            existingToken.IsUsed = true;
        }

        // Create new token
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(TokenExpirationHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailVerificationTokens.Add(verificationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated email verification token for user {UserId}. Expires at {ExpiresAt}", userId, verificationToken.ExpiresAt);

        return token;
    }

    public async Task<EmailVerificationToken?> ValidateTokenAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("ValidateTokenAsync called with null or empty token.");
            return null;
        }

        _logger.LogInformation("Validating token: {Token}", token);

        var verificationToken = await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (verificationToken == null)
        {
            _logger.LogWarning("Email verification token not found in database: {Token}", token);
            // Let's also check if it exists case-insensitively just in case
            var existsCaseInsensitive = await _context.EmailVerificationTokens.AnyAsync(t => t.Token.ToLower() == token.ToLower(), cancellationToken);
            if (existsCaseInsensitive)
            {
                _logger.LogWarning("Token exists in database but with different casing: {Token}", token);
            }
            return null;
        }

        if (verificationToken.IsUsed)
        {
            _logger.LogWarning("Email verification token already used: {Token}. Used status: {IsUsed}", token, verificationToken.IsUsed);
            return null;
        }

        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Email verification token expired: {Token}. Expired at {ExpiresAt}, Current time {CurrentTime}", 
                token, verificationToken.ExpiresAt, DateTime.UtcNow);
            return null;
        }

        _logger.LogInformation("Email verification token successfully validated for user {UserId}", verificationToken.UserId);
        return verificationToken;
    }

    public async Task InvalidateTokenAsync(string token, CancellationToken cancellationToken)
    {
        var verificationToken = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (verificationToken != null)
        {
            verificationToken.IsUsed = true;
            verificationToken.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);
            _logger.LogInformation("Invalidated email verification token: {Token}", token);
        }
    }
}

