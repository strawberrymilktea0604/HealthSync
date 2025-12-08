using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace HealthSync.Application.Services;

public interface IOtpService
{
    string GenerateOtp(string email);
    bool ValidateOtp(string email, string otp);
    void RemoveOtp(string email);
}

public class OtpService : IOtpService
{
    private readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

    public string GenerateOtp(string email)
    {
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiry = DateTime.UtcNow.AddMinutes(10); // 10 minutes
        _otpStore[email.ToLower()] = (otp, expiry);
        return otp;
    }

    public bool ValidateOtp(string email, string otp)
    {
        if (_otpStore.TryGetValue(email.ToLower(), out var stored) &&
            stored.Expiry > DateTime.UtcNow &&
            stored.Otp == otp)
        {
            _otpStore.TryRemove(email.ToLower(), out _);
            return true;
        }
        return false;
    }

    public void RemoveOtp(string email)
    {
        _otpStore.TryRemove(email.ToLower(), out _);
    }
}