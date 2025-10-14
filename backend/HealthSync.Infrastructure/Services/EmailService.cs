using HealthSync.Domain.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HealthSync.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendVerificationCodeAsync(string email, string code)
    {
        // For demo purposes, we'll just print to console
        // In production, configure SMTP settings
        Console.WriteLine($"Sending verification code {code} to {email}");

        // Uncomment below for actual email sending
        /*
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("your-email@gmail.com", "your-password"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("your-email@gmail.com"),
            Subject = "Verification Code",
            Body = $"Your verification code is: {code}",
            IsBodyHtml = false,
        };
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
        */
    }
}