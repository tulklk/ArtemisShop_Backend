using AtermisShop.Domain.Orders;

namespace AtermisShop.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string name, string verificationToken, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string email, string name, string resetToken, CancellationToken cancellationToken);
    Task SendNewPasswordAsync(string email, string name, string newPassword, CancellationToken cancellationToken);
    Task SendOrderConfirmationAsync(string email, string name, Order order, CancellationToken cancellationToken);
}

