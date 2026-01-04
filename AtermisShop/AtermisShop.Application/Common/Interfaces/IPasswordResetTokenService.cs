using AtermisShop.Domain.Users;

namespace AtermisShop.Application.Common.Interfaces;

public interface IPasswordResetTokenService
{
    Task<string> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task<PasswordResetToken?> ValidateTokenAsync(string token, CancellationToken cancellationToken);
    Task InvalidateTokenAsync(string token, CancellationToken cancellationToken);
}

