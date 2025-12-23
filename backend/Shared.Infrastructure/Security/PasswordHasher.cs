using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Shared.Infrastructure.Security;

/// <summary>
/// Secure password hashing and validation utilities
/// Uses PBKDF2 with SHA-512, which is OWASP recommended
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 64; // 512 bits
    private const int Iterations = 100000; // OWASP 2025 recommends >= 100,000 for PBKDF2-SHA512
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA512;

    /// <summary>
    /// Hashes a password using PBKDF2-SHA512 with a random salt
    /// Returns a string in format: salt:hash (both base64 encoded)
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // Generate a cryptographically secure random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: Prf,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Combine salt and hash for storage
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        try
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            // Hash the input password with the stored salt
            var inputHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random token
    /// </summary>
    public static string GenerateSecureToken(int length = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    /// <summary>
    /// Generates a secure numeric OTP code
    /// </summary>
    public static string GenerateOtpCode(int digits = 6)
    {
        if (digits < 4 || digits > 10)
            throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be between 4 and 10");

        var max = (int)Math.Pow(10, digits);
        var code = RandomNumberGenerator.GetInt32(max);
        return code.ToString().PadLeft(digits, '0');
    }
}
