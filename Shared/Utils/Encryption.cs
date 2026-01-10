namespace Shared.Utils;

using System.Security.Cryptography;
using System.Text;

public static class Encryption
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
    
    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}