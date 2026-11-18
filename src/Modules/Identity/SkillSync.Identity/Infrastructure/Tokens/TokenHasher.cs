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

        var actualHash = Convert.FromHexString(Hash(token));
        var expectedHashBytes = Convert.FromHexString(expectedHash);

        return CryptographicOperations.FixedTimeEquals(
            actualHash,
            expectedHashBytes);
    }
}