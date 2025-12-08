namespace HealthSync.Application.Services;

/// <summary>
/// Shared in-memory store for verification codes
/// In production, replace with Redis or database
/// </summary>
public static class VerificationCodeStore
{
    private static readonly Dictionary<string, VerificationEntry> _codes = new();
    private static readonly object _lock = new();

    public static void Store(string email, string code, TimeSpan? expiration = null)
    {
        lock (_lock)
        {
            var expiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(5));
            _codes[email.ToLowerInvariant()] = new VerificationEntry
            {
                Code = code,
                ExpiresAt = expiresAt,
                IsUsed = false
            };
        }
    }

    public static bool Verify(string email, string code, bool markAsUsed = true)
    {
        lock (_lock)
        {
            var normalizedEmail = email.ToLowerInvariant();
            
            if (_codes.TryGetValue(normalizedEmail, out var entry))
            {
                // Check if expired
                if (DateTime.UtcNow > entry.ExpiresAt)
                {
                    _codes.Remove(normalizedEmail);
                    return false;
                }

                // Check if already used
                if (entry.IsUsed)
                {
                    return false;
                }

                // Check if code matches
                if (entry.Code == code)
                {
                    if (markAsUsed)
                    {
                        entry.IsUsed = true;
                        // Remove after 30 seconds to prevent memory leak
                        Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(30));
                            lock (_lock)
                            {
                                _codes.Remove(normalizedEmail);
                            }
                        });
                    }
                    return true;
                }
            }

            return false;
        }
    }

    public static void Remove(string email)
    {
        lock (_lock)
        {
            _codes.Remove(email.ToLowerInvariant());
        }
    }

    private sealed class VerificationEntry
    {
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
