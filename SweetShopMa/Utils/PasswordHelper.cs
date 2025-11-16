using System;
using System.Security.Cryptography;

namespace SweetShopMa.Utils;

public static class PasswordHelper
{
    // Hashes a password using PBKDF2 with salt
    public static string HashPassword(string password)
    {
        // Generate a 16-byte salt
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            // Derive a 32-byte key using PBKDF2 (10,000 iterations, SHA-256)
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}"; // store as salt:hash
            }
        }
    }

    // Verifies an entered password against a stored salt:hash string
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);

        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
