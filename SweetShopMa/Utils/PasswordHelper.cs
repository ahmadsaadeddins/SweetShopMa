using System;
using System.Security.Cryptography;

namespace SweetShopMa.Utils;

/// <summary>
/// Provides secure password hashing and verification using PBKDF2.
/// 
/// WHAT IS PASSWORDHASHER?
/// PasswordHelper provides secure password hashing to protect user passwords.
/// Passwords are NEVER stored as plain text - they're hashed using a secure algorithm.
/// 
/// SECURITY FEATURES:
/// - PBKDF2 (Password-Based Key Derivation Function 2) algorithm
/// - SHA-256 hash function
/// - 10,000 iterations (makes brute force attacks slower)
/// - Random salt for each password (prevents rainbow table attacks)
/// - Fixed-time comparison (prevents timing attacks)
/// 
/// HOW IT WORKS:
/// 1. HashPassword: Takes plain text password, generates random salt, hashes password+salt
/// 2. Returns "salt:hash" (both Base64 encoded)
/// 3. VerifyPassword: Takes plain text password and stored "salt:hash", re-hashes and compares
/// 
/// WHY NOT PLAIN TEXT?
/// If database is compromised, attackers can't see actual passwords - only hashes.
/// Even with the hash, it's extremely difficult to reverse to get the original password.
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt.
    /// 
    /// HOW IT WORKS:
    /// 1. Generate a random 16-byte salt
    /// 2. Derive a 32-byte hash using PBKDF2 (10,000 iterations, SHA-256)
    /// 3. Return "salt:hash" (both Base64 encoded for storage)
    /// 
    /// SECURITY:
    /// - Each password gets a unique salt (prevents rainbow table attacks)
    /// - 10,000 iterations make brute force attacks very slow
    /// - SHA-256 is a cryptographically secure hash function
    /// 
    /// EXAMPLE:
    /// HashPassword("mypassword") returns something like:
    /// "dGVzdHNhbHQ=:YWJjZGVmZ2hpams=" (salt:hash in Base64)
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Hashed password in format "salt:hash" (Base64 encoded)</returns>
    public static string HashPassword(string password)
    {
        // Generate a 16-byte salt
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            // Derive a 32-byte key using PBKDF2 (configurable iterations, SHA-256)
            int iterations = AppConstants.PasswordHashIterations;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return $"{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}"; // store as iterations:salt:hash
            }
        }
    }

    /// <summary>
    /// Verifies an entered password against a stored hash.
    /// Supports both legacy format (salt:hash) with 10,000 iterations and new format (iterations:salt:hash).
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(':');
        
        int iterations = 10000; // Default for legacy hashes
        byte[] salt;
        byte[] storedHash;

        if (parts.Length == 3)
        {
            // New format: iterations:salt:hash
            if (!int.TryParse(parts[0], out iterations)) return false;
            salt = Convert.FromBase64String(parts[1]);
            storedHash = Convert.FromBase64String(parts[2]);
        }
        else if (parts.Length == 2)
        {
            // Legacy format: salt:hash
            salt = Convert.FromBase64String(parts[0]);
            storedHash = Convert.FromBase64String(parts[1]);
        }
        else
        {
            return false;
        }

        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
