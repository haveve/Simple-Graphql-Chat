using System.Security.Cryptography;
using System.Text;

namespace WebSocketGraphql.Services;

public static class PasswordHasher
{
    public static string ComputeHash(this string password, string salt, int iteration)
    {
        if (iteration <= 0) return password;

        using var sha256 = SHA256.Create();
        var passwordSaltPepper = $"{password}{salt}";
        var byteValue = Encoding.UTF8.GetBytes(passwordSaltPepper);
        var byteHash = sha256.ComputeHash(byteValue);
        var hash = Convert.ToBase64String(byteHash);
        return ComputeHash(hash, salt, iteration - 1);
    }

    public static string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var byteSalt = new byte[16];
        rng.GetBytes(byteSalt);
        var salt = Convert.ToBase64String(byteSalt);
        return salt;
    }
    public static bool ComparePasswords(this string passw1, string passw2, bool hashed = false, string salt = "", int iteration = 0)
    {
        if (hashed)
        {
            return passw1.ComputeHash(salt, iteration).Equals(passw2);
        }

        return passw1.Equals(passw2);
    }
}
