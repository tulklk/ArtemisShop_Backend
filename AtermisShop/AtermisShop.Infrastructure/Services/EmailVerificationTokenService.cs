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
            return null;

        var verificationToken = await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (verificationToken == null)
        {
            _logger.LogWarning("Email verification token not found: {Token}", token);
            return null;
        }

        if (verificationToken.IsUsed)
        {
            _logger.LogWarning("Email verification token already used: {Token}", token);
            return null;
        }

        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Email verification token expired: {Token}, Expired at {ExpiresAt}", token, verificationToken.ExpiresAt);
            return null;
        }

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

