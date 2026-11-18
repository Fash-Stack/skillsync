namespace SkillSync.Identity.Infrastructure.Tokens;

internal sealed record JwtOptions
{
    public required string Issuer { get; init; }

    public required string Audience { get; init; } 

    public required string SecretKey { get; init; } 

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public byte[] GetSigningKey()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JWT signing key is missing.");

        byte[] key;

        try
        {
            key = Convert.FromBase64String(SecretKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "JWT signing key must be Base64 encoded.",
                exception);
        }

        if (key.Length < 32)
            throw new InvalidOperationException(
                "JWT signing key must contain at least 32 bytes.");

        return key;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT issuer is missing.");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT audience is missing.");

        if (AccessTokenLifetimeMinutes <= 0)
            throw new InvalidOperationException(
                "Access token lifetime must be greater than zero.");

        _ = GetSigningKey();
    }
}