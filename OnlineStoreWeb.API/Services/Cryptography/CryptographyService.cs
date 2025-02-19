using System.Security.Cryptography;

namespace OnlineStoreWeb.API.Services.Cryptography;

public static class CryptographyService
{
public static string HashPassword(string password)
{
    int iterations = 128000; 
    int hashSize = 32; 
    int saltSize = 32; 

    byte[] salt = new byte[saltSize];
    RandomNumberGenerator.Fill(salt);

    var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
    try
    {
        byte[] hash = pbkdf2.GetBytes(hashSize);

        byte[] combined = new byte[saltSize + hashSize + 4];
        Buffer.BlockCopy(salt, 0, combined, 0, saltSize);
        Buffer.BlockCopy(hash, 0, combined, saltSize, hashSize);
        Buffer.BlockCopy(BitConverter.GetBytes(iterations), 0, combined, saltSize + hashSize, 4);

        return Convert.ToBase64String(combined);
    }
    finally
    {
        pbkdf2.Dispose();
    }
}

    public static bool TryVerifyPassword(string enteredPassword, string storedHash)
    {
        byte[] combined = Convert.FromBase64String(storedHash);

        int saltSize = 32;
        int hashSize = 32;
        int iterations = BitConverter.ToInt32(combined, saltSize + hashSize);

        byte[] salt = new byte[saltSize];
        byte[] hash = new byte[hashSize];

        Buffer.BlockCopy(combined, 0, salt, 0, saltSize);
        Buffer.BlockCopy(combined, saltSize, hash, 0, hashSize);

        var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, iterations, HashAlgorithmName.SHA256);
        try
        {
            byte[] testHash = pbkdf2.GetBytes(hashSize);
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        finally
        {
            pbkdf2.Dispose();
        }
    }
}