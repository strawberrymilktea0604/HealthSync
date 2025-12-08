using HealthSync.Domain.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HealthSync.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationCodeAsync(string email, string code)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var password = _configuration["EmailSettings:Password"];

            // For demo/development: print to console
            Console.WriteLine($"📧 Sending verification code to {email}");
            Console.WriteLine($"🔑 Verification Code: {code}");
            Console.WriteLine("-----------------------------------");

            // Try to send real email if configured
            if (!string.IsNullOrEmpty(senderEmail) && 
                !string.IsNullOrEmpty(password) && 
                senderEmail != "your-email@gmail.com")
            {
                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "HealthSync - Mã Xác Thực Đăng Ký",
                    Body = $@"
                        <h2>Chào mừng đến với HealthSync!</h2>
                        <p>Mã xác thực của bạn là:</p>
                        <h1 style='color: #4CAF50; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
                        <p>Mã này có hiệu lực trong 10 phút.</p>
                        <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                    ",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("✅ Email sent successfully!");
            }
            else
            {
                Console.WriteLine("⚠️ Email not configured. Using console output for verification code.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email: {ex.Message}");
            Console.WriteLine($"🔑 Verification Code (fallback): {code}");
        }
    }

    public async Task SendResetPasswordEmailAsync(string email, string resetToken)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var password = _configuration["EmailSettings:Password"];

            // For demo/development: print to console
            Console.WriteLine($"📧 Sending reset password email to {email}");
            Console.WriteLine($"🔗 Reset Token: {resetToken}");
            Console.WriteLine("-----------------------------------");

            // Try to send real email if configured
            if (!string.IsNullOrEmpty(senderEmail) && 
                !string.IsNullOrEmpty(password) && 
                senderEmail != "your-email@gmail.com")
            {
                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                };

                var resetLink = $"http://localhost:5173/create-new-password?token={resetToken}";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "HealthSync - Đặt Lại Mật Khẩu",
                    Body = $@"
                        <h2>Đặt Lại Mật Khẩu HealthSync</h2>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấp vào liên kết dưới đây để đặt mật khẩu mới:</p>
                        <a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đặt Lại Mật Khẩu</a>
                        <p>Liên kết này có hiệu lực trong 15 phút.</p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    ",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("✅ Reset password email sent successfully!");
            }
            else
            {
                Console.WriteLine("⚠️ Email not configured. Using console output for reset token.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send reset email: {ex.Message}");
            Console.WriteLine($"🔗 Reset Token (fallback): {resetToken}");
        }
    }
}