namespace AtermisShop.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string name, string verificationToken, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string email, string name, string resetToken, CancellationToken cancellationToken);
}

