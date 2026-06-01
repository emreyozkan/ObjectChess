using System;
using System.Security.Cryptography;

namespace ObjectChess.Business.Security
{
    // This class is responsible for password hashing and verification
    // It helps store passwords safely in the database (not plain text)
    public static class PasswordHasher
    {
        // Size of random salt (used to make hashes unique)
        private const int SaltSize = 16;

        // Size of final hash output
        private const int KeySize = 32;

        // Number of iterations for PBKDF2 (higher = more secure but slower)
        private const int Iterations = 100000;

        // Hash algorithm used internally
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        // Create hashed password from plain password
        public static string HashPassword(string password)
        {
            // Generate random salt (important for security)
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Create hash using PBKDF2 algorithm
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            // Combine salt + hash into one array
            byte[] hashBytes = new byte[SaltSize + KeySize];

            // Copy salt to beginning of array
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);

            // Copy hash after salt
            Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

            // Convert to Base64 so we can store in database as string
            return Convert.ToBase64String(hashBytes);
        }

        // Check if entered password matches stored hashed password
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Convert stored Base64 string back to byte array
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract salt from stored data
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Recalculate hash using same salt and parameters
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            // Compare computed hash with stored hash byte by byte
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    // If any byte is different, password is wrong
                    return false;
                }
            }

            // If all bytes match, password is correct
            return true;
        }
    }
}