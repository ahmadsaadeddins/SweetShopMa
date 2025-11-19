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

            // Derive a 32-byte key using PBKDF2 (10,000 iterations, SHA-256)
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}"; // store as salt:hash
            }
        }
    }

    /// <summary>
    /// Verifies an entered password against a stored hash.
    /// 
    /// HOW IT WORKS:
    /// 1. Split stored "salt:hash" string
    /// 2. Extract salt and hash (Base64 decode)
    /// 3. Hash the entered password with the same salt
    /// 4. Compare the new hash with stored hash (using fixed-time comparison)
    /// 5. Return true if they match, false otherwise
    /// 
    /// SECURITY:
    /// - Uses CryptographicOperations.FixedTimeEquals for comparison
    /// - Fixed-time comparison prevents timing attacks (attacker can't learn which byte differs)
    /// 
    /// EXAMPLE:
    /// VerifyPassword("mypassword", "dGVzdHNhbHQ=:YWJjZGVmZ2hpams=")
    /// Returns true if password matches, false otherwise
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hashedPassword">Stored hash in format "salt:hash"</param>
    /// <returns>True if password matches, false otherwise</returns>
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
