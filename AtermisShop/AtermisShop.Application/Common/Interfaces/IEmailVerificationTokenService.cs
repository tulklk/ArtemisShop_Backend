using AtermisShop.Domain.Users;

namespace AtermisShop.Application.Common.Interfaces;

public interface IEmailVerificationTokenService
{
    Task<string> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task<EmailVerificationToken?> ValidateTokenAsync(string token, CancellationToken cancellationToken);
    Task InvalidateTokenAsync(string token, CancellationToken cancellationToken);
}

