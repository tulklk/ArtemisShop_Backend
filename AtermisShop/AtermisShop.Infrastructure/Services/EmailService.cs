using AtermisShop.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AtermisShop.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _smtpFromEmail;
    private readonly string _smtpFromName;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? string.Empty;
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? string.Empty;
        _smtpFromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
        _smtpFromName = _configuration["Email:FromName"] ?? "ARTEMIS Shop";
        _frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
    }

    public async Task SendEmailVerificationAsync(string email, string name, string verificationToken, CancellationToken cancellationToken)
    {
        var verificationUrl = $"{_frontendUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        
        var subject = "Xác thực email của bạn - ARTEMIS Shop";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            margin: 0;
            padding: 0;
            background-color: #fef7f7;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #ff6b9d 0%, #ff8fab 100%);
            color: white;
            padding: 40px 20px;
            text-align: center;
            border-radius: 8px 8px 0 0;
        }}
        .header h1 {{
            margin: 0;
            font-size: 32px;
            font-weight: 700;
            letter-spacing: 1px;
        }}
        .header .subtitle {{
            margin-top: 8px;
            font-size: 14px;
            opacity: 0.95;
        }}
        .content {{
            padding: 40px 30px;
            background-color: #ffffff;
        }}
        .greeting {{
            font-size: 20px;
            color: #ff6b9d;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        .content p {{
            color: #555;
            font-size: 15px;
            margin-bottom: 15px;
        }}
        .button-wrapper {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 14px 32px;
            background: linear-gradient(135deg, #ff6b9d 0%, #ff8fab 100%);
            color: white !important;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            font-size: 16px;
            box-shadow: 0 4px 15px rgba(255, 107, 157, 0.3);
            transition: transform 0.2s;
        }}
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(255, 107, 157, 0.4);
        }}
        .link-fallback {{
            margin-top: 25px;
            padding: 15px;
            background-color: #fff5f8;
            border-left: 4px solid #ff6b9d;
            border-radius: 4px;
        }}
        .link-fallback p {{
            margin: 5px 0;
            font-size: 13px;
            color: #666;
        }}
        .link-fallback a {{
            word-break: break-all;
            color: #ff6b9d;
            text-decoration: none;
        }}
        .warning {{
            margin-top: 20px;
            padding: 12px;
            background-color: #fff9e6;
            border-left: 4px solid #ffc107;
            border-radius: 4px;
        }}
        .warning p {{
            margin: 5px 0;
            font-size: 13px;
            color: #856404;
        }}
        .footer {{
            padding: 25px 30px;
            background-color: #fef7f7;
            border-top: 1px solid #ffe5eb;
            text-align: center;
            border-radius: 0 0 8px 8px;
        }}
        .footer p {{
            margin: 5px 0;
            font-size: 13px;
            color: #999;
        }}
        .footer .signature {{
            color: #ff6b9d;
            font-weight: 600;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div style='padding: 20px 0;'>
        <div class='email-wrapper'>
            <div class='header'>
                <h1>✨ ARTEMIS ✨</h1>
                <div class='subtitle'>Vòng tay thông minh</div>
            </div>
            <div class='content'>
                <div class='greeting'>Xin chào {name}! 👋</div>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>ARTEMIS Shop</strong> - nơi bạn có thể tùy biến vòng tay GPS độc đáo cho bé với hàng trăm tùy chọn màu sắc và phụ kiện!</p>
                <p>Để hoàn tất đăng ký, vui lòng xác thực địa chỉ email của bạn bằng cách nhấp vào nút bên dưới:</p>
                
                <div class='button-wrapper'>
                    <a href='{verificationUrl}' class='button'>Xác thực email</a>
                </div>

                <div class='warning'>
                    <p>⚠️ <strong>Lưu ý:</strong> Link xác thực này sẽ hết hạn sau <strong>24 giờ</strong>.</p>
                </div>

                <p style='margin-top: 25px; font-size: 13px; color: #888;'>Nếu bạn không yêu cầu tạo tài khoản này, vui lòng bỏ qua email này và không thực hiện bất kỳ hành động nào.</p>
            </div>
            <div class='footer'>
                <p>Chúc bạn có trải nghiệm mua sắm tuyệt vời!</p>
                <div class='signature'>Đội ngũ ARTEMIS Shop 💖</div>
            </div>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, name, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string name, string resetToken, CancellationToken cancellationToken)
    {
        var resetUrl = $"{_frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        
        var subject = "Đặt lại mật khẩu - ARTEMIS Shop";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            margin: 0;
            padding: 0;
            background-color: #fef7f7;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #ff6b9d 0%, #ff8fab 100%);
            color: white;
            padding: 40px 20px;
            text-align: center;
            border-radius: 8px 8px 0 0;
        }}
        .header h1 {{
            margin: 0;
            font-size: 32px;
            font-weight: 700;
            letter-spacing: 1px;
        }}
        .header .subtitle {{
            margin-top: 8px;
            font-size: 14px;
            opacity: 0.95;
        }}
        .content {{
            padding: 40px 30px;
            background-color: #ffffff;
        }}
        .greeting {{
            font-size: 20px;
            color: #ff6b9d;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        .content p {{
            color: #555;
            font-size: 15px;
            margin-bottom: 15px;
        }}
        .button-wrapper {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 14px 32px;
            background: linear-gradient(135deg, #ff6b9d 0%, #ff8fab 100%);
            color: white !important;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            font-size: 16px;
            box-shadow: 0 4px 15px rgba(255, 107, 157, 0.3);
            transition: transform 0.2s;
        }}
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(255, 107, 157, 0.4);
        }}
        .link-fallback {{
            margin-top: 25px;
            padding: 15px;
            background-color: #fff5f8;
            border-left: 4px solid #ff6b9d;
            border-radius: 4px;
        }}
        .link-fallback p {{
            margin: 5px 0;
            font-size: 13px;
            color: #666;
        }}
        .link-fallback a {{
            word-break: break-all;
            color: #ff6b9d;
            text-decoration: none;
        }}
        .warning {{
            margin-top: 20px;
            padding: 12px;
            background-color: #fff9e6;
            border-left: 4px solid #ffc107;
            border-radius: 4px;
        }}
        .warning p {{
            margin: 5px 0;
            font-size: 13px;
            color: #856404;
        }}
        .footer {{
            padding: 25px 30px;
            background-color: #fef7f7;
            border-top: 1px solid #ffe5eb;
            text-align: center;
            border-radius: 0 0 8px 8px;
        }}
        .footer p {{
            margin: 5px 0;
            font-size: 13px;
            color: #999;
        }}
        .footer .signature {{
            color: #ff6b9d;
            font-weight: 600;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div style='padding: 20px 0;'>
        <div class='email-wrapper'>
            <div class='header'>
                <h1>✨ ARTEMIS ✨</h1>
                <div class='subtitle'>Vòng tay thông minh</div>
            </div>
            <div class='content'>
                <div class='greeting'>Xin chào {name}! 👋</div>
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>ARTEMIS Shop</strong> của bạn.</p>
                <p>Nhấp vào nút bên dưới để đặt lại mật khẩu mới:</p>
                
                <div class='button-wrapper'>
                    <a href='{resetUrl}' class='button'>🔐 Đặt lại mật khẩu</a>
                </div>

                <div class='link-fallback'>
                    <p style='margin-bottom: 8px; font-weight: 600; color: #ff6b9d;'>Hoặc copy và dán link sau vào trình duyệt:</p>
                    <p><a href='{resetUrl}'>{resetUrl}</a></p>
                </div>

                <div class='warning'>
                    <p>⚠️ <strong>Lưu ý:</strong> Link này sẽ hết hạn sau <strong>1 giờ</strong>.</p>
                </div>

                <p style='margin-top: 25px; font-size: 13px; color: #888;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này để đảm bảo an toàn cho tài khoản của bạn.</p>
            </div>
            <div class='footer'>
                <p>Chúc bạn có trải nghiệm mua sắm tuyệt vời!</p>
                <div class='signature'>Đội ngũ ARTEMIS Shop 💖</div>
                <p style='margin-top: 15px;'>🌐 <a href='{_frontendUrl}' style='color: #ff6b9d; text-decoration: none;'>{_frontendUrl}</a></p>
            </div>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, name, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning("Email configuration is missing. Email will not be sent. To: {Email}, Subject: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_smtpFromEmail, _smtpFromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully. To: {Email}, Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email. To: {Email}, Subject: {Subject}", toEmail, subject);
            throw;
        }
    }
}

