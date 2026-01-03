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
        await SendEmailAsync(email, "HealthSync - Mã Xác Thực Đăng Ký", $@"
            <h2>Chào mừng đến với HealthSync!</h2>
            <p>Mã xác thực của bạn là:</p>
            <h1 style='color: #4CAF50; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
            <p>Mã này có hiệu lực trong 10 phút.</p>
            <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
        ", code);
    }

    public async Task SendResetPasswordEmailAsync(string email, string resetToken)
    {
         // Keep for backward compatibility if needed, or implement legacy logic
         // But logic is better centralized
         var resetLink = $"http://localhost:5173/create-new-password?token={resetToken}";
         await SendEmailAsync(email, "HealthSync - Đặt Lại Mật Khẩu", $@"
            <h2>Đặt Lại Mật Khẩu HealthSync</h2>
            <p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấp vào liên kết dưới đây để đặt mật khẩu mới:</p>
            <a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đặt Lại Mật Khẩu</a>
            <p>Liên kết này có hiệu lực trong 15 phút.</p>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        ", resetToken);
    }

    public async Task SendResetPasswordOtpAsync(string email, string otp)
    {
        await SendEmailAsync(email, "HealthSync - Mã Xác Nhận Đặt Lại Mật Khẩu", $@"
            <h2>Yêu Cầu Đặt Lại Mật Khẩu</h2>
            <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản HealthSync của bạn.</p>
            <p>Mã xác nhận của bạn là:</p>
            <h1 style='color: #E53935; font-size: 32px; letter-spacing: 5px;'>{otp}</h1>
            <p>Mã này có hiệu lực trong 10 phút.</p>
            <p>Nếu bạn không yêu cầu mã này, vui lòng KHÔNG chia sẻ nó với bất kỳ ai.</p>
        ", otp);
    }

    private async Task SendEmailAsync(string email, string subject, string bodyHtml, string consoleFallback)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPortString = _configuration["EmailSettings:SmtpPort"];
            var smtpPort = int.TryParse(smtpPortString, out var port) ? port : 587;
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var password = _configuration["EmailSettings:Password"];

            // For demo/development: always print to console
            Console.WriteLine($"📧 Sending Email: {subject} -> {email}");
            Console.WriteLine($"🔑 Fallback Content: {consoleFallback}");
            Console.WriteLine("-----------------------------------");

            if (!string.IsNullOrEmpty(senderEmail) && 
                !string.IsNullOrEmpty(password) && 
                senderEmail != "your-email@gmail.com")
            {
                using var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("✅ Email sent successfully via SMTP!");
            }
            else
            {
                Console.WriteLine("⚠️ Email not configured. Using console output.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email: {ex.Message}");
            Console.WriteLine($"🔑 Fallback Content: {consoleFallback}");
        }
    }
}