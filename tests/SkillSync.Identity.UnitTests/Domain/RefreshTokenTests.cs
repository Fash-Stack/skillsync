using SkillSync.Identity.Domain;

namespace SkillSync.Identity.UnitTests;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidValues_CreatesRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "ABC123";
        var createdAt = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAt.AddDays(30);

        // Act
        var refreshToken = RefreshToken.Create(userId, tokenHash, createdAt, expiresAtUtc);

        // Assert
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(tokenHash, refreshToken.TokenHash);
        Assert.Equal(createdAt, refreshToken.CreatedAtUtc);
        Assert.Equal(expiresAtUtc, refreshToken.ExpiresAtUtc);
        Assert.False(refreshToken.IsRevoked);
        Assert.False(refreshToken.IsExpired(createdAt));
    }


    [Fact]
    public void Create_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var tokenHash = "ABC123";
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                Guid.Empty,
                tokenHash,
                createdAtUtc,
                expiresAtUtc));
    }


    [Fact]
    public void Create_WithEmptyTokenHash_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = string.Empty;

        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                userId,
                tokenHash,
                createdAtUtc,
                expiresAtUtc));
    }


    [Fact]
    public void Create_WithWhitespaceTokenHash_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "   ";

        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                userId,
                tokenHash,
                createdAtUtc,
                expiresAtUtc));
    }

    [Fact]
    public void Create_WithExpiryBeforeCreation_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "ABC123";

        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                userId,
                tokenHash,
                createdAtUtc,
                expiresAtUtc));
    }

    [Fact]
    public void Create_WithExpiryEqualToCreation_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "ABC123";

        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                userId,
                tokenHash,
                createdAtUtc,
                expiresAtUtc));
    }

    [Fact]
    public void IsExpired_WhenCurrentTimeEqualsExpiry_ReturnsTrue()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "ABC123",
            createdAtUtc,
            expiresAtUtc);

        // Act
        var result = refreshToken.IsExpired(expiresAtUtc);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);
        var nowUtc = createdAtUtc.AddDays(10);

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "ABC123",
            createdAtUtc,
            expiresAtUtc);

        // Act
        var result = refreshToken.IsActive(nowUtc);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);
        var nowUtc = expiresAtUtc;

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "ABC123",
            createdAtUtc,
            expiresAtUtc);

        // Act
        var result = refreshToken.IsActive(nowUtc);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);
        var nowUtc = createdAtUtc.AddDays(5);

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "ABC123",
            createdAtUtc,
            expiresAtUtc);

        refreshToken.Revoke(nowUtc);

        // Act
        var result = refreshToken.IsActive(nowUtc);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Revoke_WithValidTimestamp_SetsRevocationProperties()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);
        var revokedAtUtc = createdAtUtc.AddDays(5);
        var replacementHash = "NEWABC123";

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "ABC123",
            createdAtUtc,
            expiresAtUtc);

        // Act
        refreshToken.Revoke(revokedAtUtc, replacementHash);

        // Assert
        Assert.True(refreshToken.IsRevoked);
        Assert.Equal(revokedAtUtc, refreshToken.RevokedAtUtc);
        Assert.Equal(replacementHash, refreshToken.ReplacedByTokenHash);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ThrowsInvalidOperationException()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(
            2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var expiresAtUtc = createdAtUtc.AddDays(30);
        var firstRevocationTime = createdAtUtc.AddDays(5);
        var secondRevocationTime = createdAtUtc.AddDays(6);

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "ABC123",
            createdAtUtc,
            expiresAtUtc);

        refreshToken.Revoke(firstRevocationTime);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            refreshToken.Revoke(secondRevocationTime));
    }
}
