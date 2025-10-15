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
}