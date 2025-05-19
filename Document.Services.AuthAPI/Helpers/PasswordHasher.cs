using System.Security.Cryptography;
using System.Text;

namespace Document.Services.AuthAPI.Helpers;

public class PasswordHasher
{
    //simple password hashing using SHA256
    public string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return hashedPassword == HashPassword(providedPassword);
    }
}