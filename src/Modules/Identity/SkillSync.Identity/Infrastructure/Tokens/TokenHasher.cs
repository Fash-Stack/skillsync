using System.Security.Cryptography;
using System.Text;

namespace SkillSync.Identity.Infrastructure.Tokens;

internal static class TokenHasher
{
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var tokenBytes = Encoding.UTF8.GetBytes(token);

        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static bool Verify(string token, string expectedHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedHash);

        byte[] expectedHashBytes;

        try
        {
            expectedHashBytes = Convert.FromHexString(expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return expectedHashBytes.Length == actualHash.Length &&
               CryptographicOperations.FixedTimeEquals(
                   actualHash,
                   expectedHashBytes);
    }
}