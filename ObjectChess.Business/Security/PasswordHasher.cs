using System.Security.Cryptography;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Business.Security;

public class PasswordHasher : IPasswordHasher
{
    // These numbers control how the password gets hashed
    private const int SaltSize = 16; // how many random bytes the salt has
    private const int KeySize = 32; // how big the final hash is
    private const int Iterations = 100_000; // how many times we repeat the hashing
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256; // the hash algorithm we use

    public string Hash(string password)
    {
        // Make a random salt for every password
        // This way two people with the same password do not get the same hash
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        // Pbkdf2 runs the hash 100k times
        // It is slow on purpose so brute forcing becomes painful
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        // Glue the salt and the key together into one array
        // So i can save both of them as a single value
        byte[] result = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, result, 0, SaltSize);
        Array.Copy(key, 0, result, SaltSize, KeySize);

        // Turn the raw bytes into text so it fits into a normal db column
        return Convert.ToBase64String(result);
    }

    public bool Verify(string password, string hash)
    {
        // The stored hash is text so turn it back into bytes
        byte[] stored;
        try
        {
            stored = Convert.FromBase64String(hash);
        }
        catch (FormatException)
        {
            // If it is broken or not real base64 then just fail
            return false;
        }

        // If the length is wrong then it is not one of our hashes so it can not match
        if (stored.Length != SaltSize + KeySize)
        {
            return false;
        }

        // Split it back apart
        // The first 16 bytes are the salt and the rest is the original key
        byte[] salt = stored[..SaltSize];
        byte[] storedKey = stored[SaltSize..];
        // Hash the typed password with the SAME salt so the compare is fair
        byte[] computedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        // Constant time compare so an attacker can not guess the password by timing our answer
        return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
    }
}
