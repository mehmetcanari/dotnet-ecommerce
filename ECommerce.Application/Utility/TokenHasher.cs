using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Application.Utility;

public static class TokenHasher
{
    public static string HashToken(string token, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyToken(string token, byte[] salt, string storedHash)
    {
        var computedHash = HashToken(token, salt);
        return storedHash == computedHash;
    }
}
