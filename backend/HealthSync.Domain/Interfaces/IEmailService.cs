using System.Threading.Tasks;

namespace HealthSync.Domain.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code);
}