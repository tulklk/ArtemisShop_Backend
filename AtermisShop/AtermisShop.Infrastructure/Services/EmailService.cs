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

    public async Task SendNewPasswordAsync(string email, string name, string newPassword, CancellationToken cancellationToken)
    {
        var subject = "Mật khẩu mới - ARTEMIS Shop";
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
        .password-box {{
            margin: 30px 0;
            padding: 25px;
            background: linear-gradient(135deg, #fff5f8 0%, #ffe5eb 100%);
            border: 2px dashed #ff6b9d;
            border-radius: 12px;
            text-align: center;
        }}
        .password-label {{
            font-size: 14px;
            color: #666;
            margin-bottom: 10px;
            font-weight: 600;
        }}
        .password-value {{
            font-size: 32px;
            font-weight: 700;
            color: #ff6b9d;
            letter-spacing: 4px;
            font-family: 'Courier New', monospace;
            padding: 15px;
            background-color: white;
            border-radius: 8px;
            display: inline-block;
            min-width: 200px;
        }}
        .warning {{
            margin-top: 20px;
            padding: 15px;
            background-color: #fff9e6;
            border-left: 4px solid #ffc107;
            border-radius: 4px;
        }}
        .warning p {{
            margin: 5px 0;
            font-size: 13px;
            color: #856404;
        }}
        .info-box {{
            margin-top: 20px;
            padding: 15px;
            background-color: #e7f3ff;
            border-left: 4px solid #2196F3;
            border-radius: 4px;
        }}
        .info-box p {{
            margin: 5px 0;
            font-size: 13px;
            color: #0d47a1;
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
                <p>Mật khẩu mới của bạn đã được tạo. Vui lòng sử dụng mật khẩu sau để đăng nhập:</p>
                
                <div class='password-box'>
                    <div class='password-label'>Mật khẩu mới của bạn:</div>
                    <div class='password-value'>{newPassword}</div>
                </div>

                <div class='warning'>
                    <p>⚠️ <strong>Lưu ý bảo mật:</strong></p>
                    <p>• Vui lòng không chia sẻ mật khẩu này với bất kỳ ai</p>
                    <p>• Đổi mật khẩu ngay sau khi đăng nhập</p>
                    <p>• Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ với chúng tôi ngay</p>
                </div>
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

    public async Task SendOrderConfirmationAsync(string email, string name, AtermisShop.Domain.Orders.Order order, CancellationToken cancellationToken)
    {
        var paymentMethodName = order.PaymentMethod == 0 ? "COD (Thanh toán khi nhận hàng)" : "Chuyển khoản";
        var orderStatusName = order.OrderStatus switch
        {
            0 => "Chờ xử lý",
            1 => "Đã thanh toán",
            2 => "Đang xử lý",
            3 => "Đang giao hàng",
            4 => "Đã giao hàng",
            5 => "Đã hủy",
            _ => "Chờ xử lý"
        };

        // Calculate subtotal from items
        var subtotal = order.Items.Sum(item => item.LineTotal);
        const decimal shippingFee = 30000m;

        var itemsHtml = string.Join("", order.Items.Select(item => 
        {
            // Get product image - prefer primary image, otherwise first image
            var productImage = item.Product?.Images?.FirstOrDefault(img => img.IsPrimary) 
                ?? item.Product?.Images?.FirstOrDefault();
            var imageUrl = productImage?.ImageUrl ?? "";
            var imageHtml = string.IsNullOrEmpty(imageUrl) 
                ? "<div style='width: 80px; height: 80px; background-color: #f0f0f0; border-radius: 8px; display: flex; align-items: center; justify-content: center; color: #999; font-size: 12px;'>No Image</div>"
                : $"<img src='{imageUrl}' alt='{item.ProductNameSnapshot}' style='width: 80px; height: 80px; object-fit: cover; border-radius: 8px; border: 1px solid #eee;' />";
            
            return $@"
                    <tr>
                        <td data-label='Sản phẩm' style='padding: 12px; border-bottom: 1px solid #eee;'>
                            <div style='display: flex; align-items: center; gap: 12px;'>
                                <div style='flex-shrink: 0;'>
                                    {imageHtml}
                                </div>
                                <div>
                                    <strong>{item.ProductNameSnapshot}</strong>
                                    {(string.IsNullOrEmpty(item.VariantInfoSnapshot) ? "" : $"<br><small style='color: #666;'>{item.VariantInfoSnapshot}</small>")}
                                </div>
                            </div>
                        </td>
                        <td data-label='Số lượng' style='padding: 12px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                        <td data-label='Đơn giá' style='padding: 12px; border-bottom: 1px solid #eee; text-align: right;'>{item.UnitPrice:N0} ₫</td>
                        <td data-label='Thành tiền' style='padding: 12px; border-bottom: 1px solid #eee; text-align: right;'><strong>{item.LineTotal:N0} ₫</strong></td>
                    </tr>";
        }));

        var shippingAddress = $"{order.ShippingAddressLine}, {order.ShippingDistrict}, {order.ShippingCity}";
        if (string.IsNullOrEmpty(order.ShippingAddressLine))
            shippingAddress = "Chưa cập nhật";

        var subject = $"Xác nhận đơn hàng #{order.OrderNumber} - ARTEMIS Shop";
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
        .order-info {{
            background-color: #fff5f8;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            border-left: 4px solid #ff6b9d;
        }}
        .order-info p {{
            margin: 8px 0;
            color: #555;
            font-size: 14px;
            word-break: break-word;
        }}
        .order-info strong {{
            color: #ff6b9d;
        }}
        .items-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        .items-table th {{
            background-color: #ff6b9d;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
            font-size: 14px;
        }}
        .items-table td {{
            padding: 12px;
            border-bottom: 1px solid #eee;
            font-size: 14px;
        }}
        .total-section {{
            margin-top: 20px;
            padding: 20px;
            border-radius: 8px;
            border-left: 4px solid #ff6b9d;
        }}
        .total-row {{
            display: flex;
            justify-content: space-between;
            margin: 8px 0;
            padding: 8px 0;
            border-bottom: 1px solid #eee;
            color: #555;
            font-size: 14px;
        }}
        .total-row:last-child {{
            border-bottom: none;
            font-size: 18px;
            font-weight: 700;
            color: #ff6b9d;
            margin-top: 12px;
            padding-top: 12px;
            border-top: 2px solid rgba(255, 107, 157, 0.3);
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
        /* Mobile Responsive */
        @media only screen and (max-width: 600px) {{
            .email-wrapper {{
                width: 100% !important;
                margin: 0 !important;
            }}
            .content {{
                padding: 20px 15px !important;
            }}
            .header {{
                padding: 30px 15px !important;
            }}
            .header h1 {{
                font-size: 24px !important;
            }}
            .order-info {{
                padding: 15px !important;
            }}
            .order-info p {{
                font-size: 13px !important;
            }}
            .items-table {{
                font-size: 12px !important;
                display: block;
                overflow-x: auto;
                -webkit-overflow-scrolling: touch;
            }}
            .items-table th,
            .items-table td {{
                padding: 8px 6px !important;
                font-size: 12px !important;
            }}
            .items-table th:nth-child(2),
            .items-table th:nth-child(3),
            .items-table th:nth-child(4),
            .items-table td:nth-child(2),
            .items-table td:nth-child(3),
            .items-table td:nth-child(4) {{
                min-width: 70px !important;
            }}
            .total-section {{
                padding: 15px !important;
            }}
            .total-row {{
                font-size: 13px !important;
            }}
            .total-row:last-child {{
                font-size: 16px !important;
            }}
            .footer {{
                padding: 20px 15px !important;
            }}
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
                <p>Cảm ơn bạn đã đặt hàng tại <strong>ARTEMIS Shop</strong>! Chúng tôi đã nhận được đơn hàng của bạn và đang xử lý.</p>
                
                <div class='order-info'>
                    <p><strong>Mã đơn hàng:</strong> #{order.OrderNumber}</p>
                    <p><strong>Trạng thái:</strong> {orderStatusName}</p>
                    <p><strong>Phương thức thanh toán:</strong> {paymentMethodName}</p>
                    <p><strong>Địa chỉ giao hàng:</strong> {shippingAddress}</p>
                    {(string.IsNullOrEmpty(order.ShippingPhoneNumber) ? "" : $"<p><strong>Số điện thoại:</strong> {order.ShippingPhoneNumber}</p>")}
                </div>

                <h3 style='color: #ff6b9d; margin-top: 30px;'>Chi tiết đơn hàng:</h3>
                <table class='items-table' style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                    <thead>
                        <tr>
                            <th style='text-align: left; padding: 12px; background-color: #ff6b9d; color: white; font-weight: 600;'>Sản phẩm</th>
                            <th style='text-align: center; padding: 12px; background-color: #ff6b9d; color: white; font-weight: 600; width: 80px;'>Số lượng</th>
                            <th style='text-align: right; padding: 12px; background-color: #ff6b9d; color: white; font-weight: 600; width: 120px;'>Đơn giá</th>
                            <th style='text-align: right; padding: 12px; background-color: #ff6b9d; color: white; font-weight: 600; width: 120px;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>

                <div class='total-section'>
                    <div class='total-row'>
                        <span>Tạm tính ({order.Items.Count} sản phẩm) : </span>
                        <span> {subtotal:N0}₫</span>
                    </div>
                    {(order.VoucherDiscountAmount.HasValue && order.VoucherDiscountAmount > 0 ? $@"
                    <div class='total-row'>
                        <span>Giảm giá : </span>
                        <span style='color: #4ade80;'>-{order.VoucherDiscountAmount.Value:N0}₫</span>
                    </div>" : "")}
                    <div class='total-row'>
                        <span>Phí vận chuyển : </span>
                        <span> {shippingFee:N0}₫</span>
                    </div>
                    <div class='total-row'>
                        <span><strong>Tổng cộng : </strong></span>
                        <span><strong> {order.TotalAmount:N0}₫</strong></span>
                    </div>
                </div>

                <p style='margin-top: 25px; font-size: 13px; color: #888;'>
                    {(order.PaymentMethod == 0 
                        ? "Đơn hàng của bạn sẽ được giao trong vòng 3-5 ngày làm việc. Bạn sẽ thanh toán khi nhận hàng." 
                        : "Đơn hàng của bạn đã được thanh toán thành công. Chúng tôi sẽ giao hàng trong vòng 3-5 ngày làm việc.")}
                </p>
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

