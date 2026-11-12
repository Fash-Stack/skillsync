namespace SkillSync.Identity.Domain;

internal sealed class RefreshToken
{
    private RefreshToken(
    Guid userId,
    string tokenHash,
    DateTimeOffset createdAtUtc,
    DateTimeOffset expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsExpired(DateTimeOffset nowUtc)
        => nowUtc >= ExpiresAtUtc;

    public bool IsActive(DateTimeOffset nowUtc)
        => !IsRevoked && !IsExpired(nowUtc);

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException(
                "Token hash is required.", nameof(tokenHash));

        createdAtUtc = createdAtUtc.ToUniversalTime();
        expiresAtUtc = expiresAtUtc.ToUniversalTime();

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException(
                "Expiry must be after creation.");

        return new RefreshToken(
            userId,
            tokenHash,
            createdAtUtc,
            expiresAtUtc);
    }

    public void Revoke(
        DateTimeOffset revokedAtUtc,
        string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException(
                "Token has already been revoked.");

        revokedAtUtc = revokedAtUtc.ToUniversalTime();

        if (revokedAtUtc < CreatedAtUtc)
            throw new ArgumentException(
                "Revocation time cannot be before creation time.",
                nameof(revokedAtUtc));

        if (replacedByTokenHash is not null &&
            string.IsNullOrWhiteSpace(replacedByTokenHash))
            throw new ArgumentException(
                "Replacement token hash cannot be empty.",
                nameof(replacedByTokenHash));

        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}